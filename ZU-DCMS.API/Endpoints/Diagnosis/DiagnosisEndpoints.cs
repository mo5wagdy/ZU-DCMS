using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents;

namespace ZU_DCMS.API.Endpoints.Diagnosis
{
    /// <summary>
    /// Extension class to register Diagnosis clinical endpoints.
    /// </summary>
    public static class DiagnosisEndpoints
    {
        public static void MapDiagnosisEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/diagnosis")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Diagnosis")
                           // Protect via Clinical Core Policy (InternDoctor, Receptionist, Admin)
                           .RequireAuthorization("ClinicalCorePolicy"); 

            // 1. Log a diagnosis
            group.MapPost("/", async (ISender sender, DiagnosePatientCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Patient diagnosis recorded successfully."));
            })
            .WithName("DiagnosePatient")
            .WithSummary("Records a new clinical diagnosis assessment for a queued patient");

            // 2. See which students need this case constraint
            group.MapGet("/available-students", async (ISender sender, [AsParameters] GetAvailableStudentsQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Matching available students retrieved."));
            })
            .WithName("GetAvailableStudents")
            .WithSummary("Finds active students who actually need the specific procedure requirement");

            // 3. Assign
            group.MapPost("/assign", async (ISender sender, AssignStudentCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Student was successfully assigned to this patient's case."));
            })
            .WithName("AssignStudent")
            .WithSummary("Binds a clinical case to a specific student that needed it");
        }
    }
}
