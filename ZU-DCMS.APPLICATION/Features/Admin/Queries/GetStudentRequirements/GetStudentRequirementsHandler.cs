using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetStudentRequirements
{
    public class GetStudentRequirementsHandler : IRequestHandler<GetStudentRequirementsQuery, Result<List<StudentRequirementDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetStudentRequirementsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<StudentRequirementDto>>> Handle(GetStudentRequirementsQuery query, CancellationToken cancellationToken)
        {
            // __ Fetch requirements where studentId + termId __ //
            var requirements = await _uow.Repository<TermRequirement>().GetListAsync
                (r => 
                    r.StudentId == query.StudentId &&
                    r.TermId == query.TermId,
                    false,
                    r => r.Clinic,
                    r => r.CompletedCount
                );

            // __ Map to DTO __ //
            return Result.Success(_mapper.Map<List<StudentRequirementDto>>(requirements));
        }
    }
}
