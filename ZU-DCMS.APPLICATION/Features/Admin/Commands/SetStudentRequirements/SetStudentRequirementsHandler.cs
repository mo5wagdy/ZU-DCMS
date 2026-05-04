using MediatR;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements
{
    public class SetStudentRequirementsHandler : IRequestHandler<SetStudentRequirementsCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFusionCache _cache;
        private readonly ILogger<SetStudentRequirementsHandler> _logger;

        public SetStudentRequirementsHandler
        (
            IUnitOfWork uow,
            IFusionCache cache,
            ILogger<SetStudentRequirementsHandler> logger
        )
        {
            _uow = uow;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result> Handle(SetStudentRequirementsCommand command, CancellationToken cancellationToken)
        {
            // __ Verify student exists __ //
            var student = await _uow.Repository<Student>().GetByIdAsync(command.StudentId);

            if (student is null)
                return Result.Failure("الطالب غير موجود");

            // __ Verify term exists __ //
            var term = await _uow.Repository<Term>().GetByIdAsync(command.TermId);

            if (term is null)
                return Result.Failure("الترم غير موجود");

            // __ Verify all clinics exist __ //
            var clinicIds = command.Requirements.Select(r => r.ClinicId).ToList();
            
            var clinics = await _uow.Repository<Clinic>().GetListAsync(c => clinicIds.Contains(c.Id) && c.IsActive);

            if (clinics.Count != clinicIds.Distinct().Count())
                return Result.Failure("بعض العيادات غير موجودة أو غير نشطة");

            // __ Block Diagnosis Clinic (ID: 1) from having student requirements __ //
            if (clinicIds.Contains(1))
                return Result.Failure("لا يمكن إضافة متطلبات لعيادة التشخيص");

            // __ Validate Student's Academic Year against each Clinic's constraints __ //
            foreach (var reqDto in command.Requirements)
            {
                var clinic = clinics.FirstOrDefault(c => c.Id == reqDto.ClinicId);
                
                if (clinic != null && (student.AcademicYear < clinic.MinAcademicYear || student.AcademicYear > clinic.MaxAcademicYear))
                {
                    return Result.Failure($"العيادة '{clinic.Name}' غير متاحة لطلاب السنة {student.AcademicYear} (المطلوب: {clinic.MinAcademicYear}-{clinic.MaxAcademicYear})");
                }
            }

            await _uow.BeginTransactionAsync();

            try
            {
                // __ Load existing requirements for this student + term __ //
                var existing = await _uow.Repository<TermRequirement>().GetListAsync
                    (
                        r =>
                        r.StudentId == command.StudentId &&
                        r.TermId == command.TermId
                    );

                // __ Soft delete existing requirements __ //
                foreach (var req in existing)
                    _uow.Repository<TermRequirement>().SoftDelete(req);

                // __ Create new requirements __ //
                var newRequirements = command.Requirements.Select(r =>
                    new TermRequirement
                    {
                        StudentId = command.StudentId,
                        TermId = command.TermId,
                        ClinicId = r.ClinicId,
                        AcademicYear = student.AcademicYear,
                        RequiredCount = r.RequiredCount,
                        CompletedCount = 0,
                        CreatedAt = DateTime.UtcNow
                    });

                await _uow.Repository<TermRequirement>().AddRangeAsync(newRequirements);

                await _uow.CommitTransactionAsync(command.AdminId);

                // __ Cache Invalidation __ //
                await _cache.RemoveAsync(CacheKeys.StudentRequirements(command.StudentId, command.TermId));
                await _cache.RemoveAsync(CacheKeys.StudentProgress(command.StudentId, command.TermId));

                return Result.Success();
            }

            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();

                _logger.LogError(ex, "Error setting requirements for student {StudentId}", command.StudentId);
                
                return Result.Failure("حدث خطأ أثناء تحديث المتطلبات");
            }
        }
    }
}