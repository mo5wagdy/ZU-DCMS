using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllClinics
{
    public class GetAllClinicsHandler : IRequestHandler<GetAllClinicsQuery, Result<List<ClinicDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAllClinicsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<ClinicDto>>> Handle(GetAllClinicsQuery request, CancellationToken cancellationToken)
        {
            var clinics = await _uow.Repository<Clinic>().GetListAsync(c => !c.IsDeleted);
            return Result.Success(_mapper.Map<List<ClinicDto>>(clinics));
        }
    }
}
