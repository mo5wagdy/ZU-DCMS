using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetDiagnosisByBooking
{
    public class GetDiagnosisByBookingHandler : IRequestHandler<GetDiagnosisByBookingQuery, Result<DiagnosisRecordDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetDiagnosisByBookingHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<DiagnosisRecordDto>> Handle(GetDiagnosisByBookingQuery query, CancellationToken cancellationToken)
        {
            var diagnosis = await _uow.Repository<DiagnosisRecord>().GetFirstOrDefaultAsync
                (
                    d => d.BookingId == query.BookingId,
                    true,
                    d => d.Booking,
                    d => d.Booking.Patient,
                    d => d.InternDoctor,
                    d => d.Clinic,
                    d => d.DiagnosisType
                );

            if (diagnosis == null)
            {
                return Result.Failure<DiagnosisRecordDto>("Diagnosis not found for this booking");
            }

            return Result.Success(_mapper.Map<DiagnosisRecordDto>(diagnosis));
        }
    }
}
