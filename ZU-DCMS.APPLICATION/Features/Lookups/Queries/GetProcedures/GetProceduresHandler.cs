using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetProcedures
{
    public class GetProceduresHandler : IRequestHandler<GetProceduresQuery, Result<List<ProcedureDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetProceduresHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<List<ProcedureDto>>> Handle(GetProceduresQuery request, CancellationToken cancellationToken)
        {
            List<Procedure> procedures;

            if (request.ClinicId.HasValue)
            {
                var links = await _uow.Repository<ClinicProcedure>().GetListAsync(
                    x => x.ClinicId == request.ClinicId.Value && x.IsActive,
                    true,
                    x => x.Procedure
                );

                procedures = links
                    .Select(x => x.Procedure)
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .ToList();
            }
            else
            {
                procedures = (await _uow.Repository<Procedure>().GetListAsync(p => p.IsActive && !p.IsDeleted)).ToList();
            }

            var dtos = procedures.Select(p => new ProcedureDto
            {
                Id = p.Id,
                Code = p.Code,
                NameAr = p.NameAr,
                NameEn = p.NameEn
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
