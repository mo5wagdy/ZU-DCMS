using MediatR;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetDiagnosisTypes
{
    public class GetDiagnosisTypesHandler : IRequestHandler<GetDiagnosisTypesQuery, Result<List<DiagnosisTypeDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetDiagnosisTypesHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<List<DiagnosisTypeDto>>> Handle(GetDiagnosisTypesQuery query, CancellationToken cancellationToken)
        {
            List<DiagnosisType> types;

            if (query.ClinicId.HasValue)
            {
                var links = await _uow.Repository<ClinicDiagnosisType>().GetListAsync(
                    d => d.ClinicId == query.ClinicId.Value && d.IsActive,
                    true,
                    d => d.DiagnosisType
                );

                types = links
                    .Select(x => x.DiagnosisType)
                    .Where(d => d.IsActive && !d.IsDeleted)
                    .ToList();
            }
            else
            {
                types = (await _uow.Repository<DiagnosisType>().GetListAsync(d => d.IsActive && !d.IsDeleted)).ToList();
            }

            var dtos = types.Select(t => new DiagnosisTypeDto
            {
                Id = t.Id,
                Code = t.Code,
                NameAr = t.NameAr,
                NameEn = t.NameEn
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
