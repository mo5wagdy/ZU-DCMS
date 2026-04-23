using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Patients.Commands.UpdateProfile;
using ZU_DCMS.APPLICATION.Features.Patients.Queries.GetAllPatients;
using ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientById;
using ZU_DCMS.APPLICATION.Features.Patients.Queries.GetPatientByUserId;

namespace ZU_DCMS.API.Endpoints.Patients
{
    /// <summary>
    /// Extension class to register Patient management endpoints.
    /// </summary>
    public static class PatientEndpoints
    {
        public static void MapPatientEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/patients")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Patients");

            // 1. Patient edits their own base info
            group.MapPut("/profile", async (ISender sender, UpdateProfileCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Patient profile successfully updated."));
            })
            .RequireAuthorization("PatientPolicy") // Restrict primarily to Patients making modifications
            .WithName("UpdatePatientProfile")
            .WithSummary("Modifies the authenticated patient's profile details");

            // 2. List all globally
            group.MapGet("/", async ([AsParameters] PagedRequest request, ISender sender) =>
            {
                var query = new GetAllPatientsQuery(request);
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Patients directory populated."));
            })
            .RequireAuthorization("StaffViewPolicy") // Protect standard access strictly for internal personnel
            .WithName("GetAllPatients")
            .WithSummary("Retrieves all registered external clinical patients");

            // 3. Drilldown to specific Identity string
            group.MapGet("/{id}", async (ISender sender, [AsParameters] GetPatientByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Patient metrics retrieved."));
            })
            .RequireAuthorization("StaffViewPolicy")
            .WithName("GetPatientById")
            .WithSummary("Pulls patient base identity using their absolute Database ID");

            // 4. Retrieve by app user link
            group.MapGet("/user/{id}", async (ISender sender, [AsParameters] GetPatientByUserIdQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Patient records processed from the underlying User framework."));
            })
            .RequireAuthorization() // Should be secured
            .WithName("GetPatientByUserId")
            .WithSummary("Links application User Identity to the Patient's profile data safely");
        }
    }
}
