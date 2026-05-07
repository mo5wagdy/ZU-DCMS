using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Enums;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentTodayPatients
{
    public class GetStudentTodayPatientsHandler : IRequestHandler<GetStudentTodayPatientsQuery, Result<List<CaseAssignmentDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetStudentTodayPatientsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<CaseAssignmentDto>>> Handle(GetStudentTodayPatientsQuery request, CancellationToken cancellationToken)
        {
            var student = await _uow.Repository<Student>().GetFirstOrDefaultAsync(s => s.ApplicationUserId == request.StudentApplicationUserId);
            if (student == null)
                return Result.Failure<List<CaseAssignmentDto>>("Student not found");

            // __ Find CaseAssignments that have a confirmed/delayed booking for today __ //
            var cases = await _uow.Repository<CaseAssignment>().GetListAsync(
                c => c.StudentId == student.Id &&
                     c.Status == CaseStatus.InProgress &&
                     c.DiagnosisRecord!.Booking!.Session!.Date.Date == DateTime.Today &&
                     (c.DiagnosisRecord!.Booking!.Status == BookingStatus.Confirmed || c.DiagnosisRecord!.Booking!.Status == BookingStatus.Delayed),
                false,
                c => c.DiagnosisRecord.Booking.Patient,
                c => c.DiagnosisRecord.Booking.Session, // Added Session include
                c => c.DiagnosisRecord.DiagnosisType,
                c => c.Clinic,
                c => c.Sessions
            );

            // __ Also find those where the FOLLOW-UP booking is for today __ //
            var followUpBookings = await _uow.Repository<Booking>().GetListAsync
            (
                b => b.CaseAssignmentId != null &&
                     b.CaseAssignment!.StudentId == student.Id &&
                     b.BookingType == BookingType.FollowUp &&
                     b.Session!.Date.Date == DateTime.Today &&
                     (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Delayed),
                false,
                b => b.Session, // Added Session include
                b => b.CaseAssignment!,
                b => b.CaseAssignment!.DiagnosisRecord.Booking.Patient,
                b => b.CaseAssignment!.DiagnosisRecord.DiagnosisType,
                b => b.CaseAssignment!.Clinic,
                b => b.CaseAssignment!.Sessions
            );

            // __ Merge and Map __ //
            var allCases = cases.ToList();
            foreach (var fb in followUpBookings)
            {
                if (fb.CaseAssignment != null && !allCases.Any(c => c.Id == fb.CaseAssignment.Id))
                {
                    allCases.Add(fb.CaseAssignment!);
                }
            }

            return Result.Success(_mapper.Map<List<CaseAssignmentDto>>(allCases));
        }
    }
}
