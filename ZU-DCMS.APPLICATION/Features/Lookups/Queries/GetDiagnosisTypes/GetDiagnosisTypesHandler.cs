using MediatR;
using ZU_DCMS.APPLICATION.Common;
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

        public async Task<Result<List<DiagnosisTypeDto>>> Handle(GetDiagnosisTypesQuery request, CancellationToken cancellationToken)
        {
            var types = await _uow.Repository<DiagnosisType>().GetListAsync(
                t => (!request.ClinicId.HasValue || t.ClinicId == request.ClinicId.Value) && !t.IsDeleted
            );

            var dtos = types.Select(t => new DiagnosisTypeDto
            {
                Id = t.Id,
                Name = t.NameAr, // Using Arabic name as per DB seed
                ClinicId = t.ClinicId
            }).ToList();

            return Result.Success(dtos);
        }
    }
}
