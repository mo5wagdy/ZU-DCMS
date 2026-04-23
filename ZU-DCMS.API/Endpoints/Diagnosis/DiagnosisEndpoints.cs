using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Student;

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
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<DiagnosisRecordDto>.Success(result.Value, "Patient diagnosis recorded successfully."))
                    : Results.BadRequest(ApiResponse<DiagnosisRecordDto>.Failure(result.Error, "Failed to record diagnosis."));
            })
            .WithName("DiagnosePatient")
            .WithSummary("Records a new clinical diagnosis assessment for a queued patient")
            .Produces<ApiResponse<DiagnosisRecordDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DiagnosisRecordDto>>(StatusCodes.Status400BadRequest);

            // 2. See which students need this case constraint
            group.MapGet("/available-students", async (ISender sender, [AsParameters] GetAvailableStudentsQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<StudentPriorityDto>>.Success(result.Value, "Matching available students retrieved."))
                    : Results.BadRequest(ApiResponse<List<StudentPriorityDto>>.Failure(result.Error, "Failed to retrieve available students."));
            })
            .WithName("GetAvailableStudents")
            .WithSummary("Finds active students who actually need the specific procedure requirement")
            .Produces<ApiResponse<List<StudentPriorityDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<StudentPriorityDto>>>(StatusCodes.Status400BadRequest);

            // 3. Assign
            group.MapPost("/assign", async (ISender sender, AssignStudentCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<CaseAssignmentDto>.Success(result.Value, "Student was successfully assigned to this patient's case."))
                    : Results.BadRequest(ApiResponse<CaseAssignmentDto>.Failure(result.Error, "Failed to assign student."));
            })
            .WithName("AssignStudent")
            .WithSummary("Binds a clinical case to a specific student that needed it")
            .Produces<ApiResponse<CaseAssignmentDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CaseAssignmentDto>>(StatusCodes.Status400BadRequest);
        }
    }
}
