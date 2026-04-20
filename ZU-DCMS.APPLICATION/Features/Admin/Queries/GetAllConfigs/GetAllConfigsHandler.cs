using AutoMapper;
using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllConfigs
{
    public class GetAllConfigsHandler : IRequestHandler<GetAllConfigsQuery, Result<List<SystemConfigDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAllConfigsHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<SystemConfigDto>>> Handle(GetAllConfigsQuery query, CancellationToken cancellationToken)
        {
            // __ Fetch all configs from SystemConfig table __ //
            var configs = await _uow.Repository<SystemConfig>().GetListAsync();

            // __ Map to DTO __ //
            return Result.Success(_mapper.Map<List<SystemConfigDto>>(configs));
        }
    }
}
