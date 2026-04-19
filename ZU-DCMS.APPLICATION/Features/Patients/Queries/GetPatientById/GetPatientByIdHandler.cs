using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.DTOs.Patient;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientById
{
    public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAppLogger<GetPatientByIdHandler> _logger;

        public GetPatientByIdHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IAppLogger<GetPatientByIdHandler> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        // ________________ Get By Patient Id ________________ //
        public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery query, CancellationToken cancellationToken)
        {
            var id = query.Id;

            _logger.LogInfo("Fetching patient by ID: {Id}", id);

            // __ Fetching by id __ // 
            var patient = await _uow.Repository<Patient>().GetByIdAsync(id);

            if (patient is null)
            {
                _logger.LogWarning("Patient not found with ID: {Id}", id);
               
                return Result.Failure<PatientDto>("المريض غير موجود");
            }
            
            _logger.LogInfo("Patient found: {FullName} (ID: {Id})", patient.FullName, patient.Id);

            return Result.Success<PatientDto>(_mapper.Map<PatientDto>(patient));
        }
    }
}
