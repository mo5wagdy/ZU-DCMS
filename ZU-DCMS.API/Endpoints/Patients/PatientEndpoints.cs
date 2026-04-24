using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Patients.Commands.UpdateProfile;
using ZU_DCMS.APPLICATION.Features.Patients.Queries.GetAllPatients;
using ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientById;
using ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientByUserId;
using ZU_DCMS.APPLICATION.DTOs.Patient;

namespace ZU_DCMS.API.Endpoints.Patients
{
    /// <summary>
    /// Extension class to register Patient management endpoints.
    /// </summary>
    public static class PatientEndpoints
    {
        public static void MapPatientEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/patients")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Patients");

            // 1. Patient edits their own base info
            group.MapPut("/profile", async ([FromServices] ISender sender, [FromBody] UpdateProfileCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<UpdatePatientDto>.Success(result.Value, "Patient profile successfully updated."))
                    : Results.BadRequest(ApiResponse<UpdatePatientDto>.Failure(result.Error, "Failed to update patient profile."));
            })
            .RequireAuthorization("PatientPolicy") // Restrict primarily to Patients making modifications
            .WithName("UpdatePatientProfile")
            .WithSummary("Modifies the authenticated patient's profile details")
            .Produces<ApiResponse<UpdatePatientDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdatePatientDto>>(StatusCodes.Status400BadRequest);

            // 2. List all globally
            group.MapGet("/", async ([AsParameters] PagedRequest request, [FromServices] ISender sender) =>
            {
                var query = new GetAllPatientsQuery(request);
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PagedResult<PatientDto>>.Success(result.Value, "Patients directory populated."))
                    : Results.BadRequest(ApiResponse<PagedResult<PatientDto>>.Failure(result.Error, "Failed to retrieve patients."));
            })
            .RequireAuthorization("StaffViewPolicy") // Protect standard access strictly for internal personnel
            .WithName("GetAllPatients")
            .WithSummary("Retrieves all registered external clinical patients")
            .Produces<ApiResponse<PagedResult<PatientDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PagedResult<PatientDto>>>(StatusCodes.Status400BadRequest);

            // 3. Drilldown to specific Identity string
            group.MapGet("/{id}", async ([FromServices] ISender sender, [AsParameters] GetPatientByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PatientDto>.Success(result.Value, "Patient metrics retrieved."))
                    : Results.NotFound(ApiResponse<PatientDto>.Failure(result.Error, "Patient not found."));
            })
            .RequireAuthorization("StaffViewPolicy")
            .WithName("GetPatientById")
            .WithSummary("Pulls patient base identity using their absolute Database ID")
            .Produces<ApiResponse<PatientDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PatientDto>>(StatusCodes.Status404NotFound);

            // 4. Retrieve by app user link
            group.MapGet("/user/{userId}", async ([FromServices] ISender sender, [AsParameters] GetPatientByUserIdQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PatientDto>.Success(result.Value, "Patient records processed from the underlying User framework."))
                    : Results.NotFound(ApiResponse<PatientDto>.Failure(result.Error, "Patient records not found by specified user ID."));
            })
            .RequireAuthorization() // Should be secured
            .WithName("GetPatientByUserId")
            .WithSummary("Links application User Identity to the Patient's profile data safely")
            .Produces<ApiResponse<PatientDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PatientDto>>(StatusCodes.Status404NotFound);
        }
    }
}
