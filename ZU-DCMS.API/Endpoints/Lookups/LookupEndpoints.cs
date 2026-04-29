using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllClinics;
using ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetDiagnosisTypes;

namespace ZU_DCMS.API.Endpoints.Lookups
{
    public static class LookupEndpoints
    {
        public static void MapLookupEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/lookups")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Lookups")
                           .RequireAuthorization("PublicViewPolicy");

            group.MapGet("/clinics", async ([FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetAllClinicsQuery());
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<ClinicDto>>.Success(result.Value, "Clinics retrieved."))
                    : Results.BadRequest(ApiResponse<List<ClinicDto>>.Failure(result.Errors, "Failed to retrieve clinics."));
            })
            .WithName("GetClinicsLookup")
            .Produces<ApiResponse<List<ClinicDto>>>(StatusCodes.Status200OK);

            group.MapGet("/diagnosis-types", async ([FromQuery] int? clinicId, [FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetDiagnosisTypesQuery(clinicId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<DiagnosisTypeDto>>.Success(result.Value, "Diagnosis types retrieved."))
                    : Results.BadRequest(ApiResponse<List<DiagnosisTypeDto>>.Failure(result.Errors, "Failed to retrieve diagnosis types."));
            })
            .WithName("GetDiagnosisTypesLookup")
            .Produces<ApiResponse<List<DiagnosisTypeDto>>>(StatusCodes.Status200OK);
        }
    }
}
