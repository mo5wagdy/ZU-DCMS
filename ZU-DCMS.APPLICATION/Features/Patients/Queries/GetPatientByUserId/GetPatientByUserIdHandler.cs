using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientByUserId
{
    public class GetPatientByUserIdHandler : IRequestHandler<GetPatientByUserIdQuery, Result<PatientDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPatientByUserIdHandler> _logger;

        public GetPatientByUserIdHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetPatientByUserIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // ________________ Get By Identity User Id ________________ //
        public async Task<Result<PatientDto>> Handle(GetPatientByUserIdQuery query, CancellationToken cancellationToken)
        {
            var userId = query.UserId;

            _logger.LogInfo("Fetching patient by User ID: {UserId}", userId);

            // __ Fetching by app user id using provided id __ //
            var patient = await _uow.Repository<Domain.Entities.Patient>().GetFirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            // __ If null return failure __ //
            if (patient is null)
            {
                _logger.LogWarning("Patient not found with User ID: {UserId}", userId);
                
                return Result.Failure<PatientDto>("المريض غير موجود");
            }
            
            _logger.LogInfo("Patient found: {FullName} (User ID: {UserId})", patient.FullName, userId);

            return Result.Success<PatientDto>(_mapper.Map<PatientDto>(patient));
        }
    }
}
