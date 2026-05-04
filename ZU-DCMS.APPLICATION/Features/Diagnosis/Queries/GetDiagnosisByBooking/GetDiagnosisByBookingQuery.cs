using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetDiagnosisByBooking
{
    public record GetDiagnosisByBookingQuery(int BookingId) : IRequest<Result<DiagnosisRecordDto>>;
}
