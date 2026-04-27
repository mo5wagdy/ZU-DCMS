using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Diagnosis;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.AssignStudent;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Commands.DiagnosePatient;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetAvailableStudents;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Queries.GetSessionPatients;
using ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetDiagnosisTypes;
using ZU_DCMS.APPLICATION.Features.Lookups.Queries.GetProcedures;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.API.Endpoints.Diagnosis
{
    /// <summary>
    /// Extension class to register Diagnosis clinical endpoints.
    /// </summary>
    public static class DiagnosisEndpoints
    {
        public static void MapDiagnosisEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/diagnosis")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Diagnosis");
                           // Protect via Clinical Core Policy (InternDoctor, Receptionist, Admin)
                           //.RequireAuthorization("ClinicalCorePolicy"); 

            // 1. Log a diagnosis
            group.MapPost("/", async ([FromServices] ISender sender, [FromBody] DiagnosePatientCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<DiagnosisRecordDto>.Success(result.Value, "Patient diagnosis recorded successfully."))
                    : Results.BadRequest(ApiResponse<DiagnosisRecordDto>.Failure(result.Errors, "Failed to record diagnosis."));
            })
            .WithName("DiagnosePatient")
            .WithSummary("Records a new clinical diagnosis assessment for a queued patient")
            .Produces<ApiResponse<DiagnosisRecordDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DiagnosisRecordDto>>(StatusCodes.Status400BadRequest);

            // 2. See which students need this case constraint
            group.MapGet("/available-students", async ([FromServices] ISender sender, [AsParameters] GetAvailableStudentsQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<StudentPriorityDto>>.Success(result.Value, "Matching available students retrieved."))
                    : Results.BadRequest(ApiResponse<List<StudentPriorityDto>>.Failure(result.Errors, "Failed to retrieve available students."));
            })
            .WithName("GetAvailableStudents")
            .WithSummary("Finds active students who actually need the specific procedure requirement")
            .Produces<ApiResponse<List<StudentPriorityDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<StudentPriorityDto>>>(StatusCodes.Status400BadRequest);

            // 3. Assign
            group.MapPost("/assign", async ([FromServices] ISender sender, [FromBody] AssignStudentCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<CaseAssignmentDto>.Success(result.Value, "Student was successfully assigned to this patient's case."))
                    : Results.BadRequest(ApiResponse<CaseAssignmentDto>.Failure(result.Errors, "Failed to assign student."));
            })
            .WithName("AssignStudent")
            .WithSummary("Binds a clinical case to a specific student that needed it")
            .Produces<ApiResponse<CaseAssignmentDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CaseAssignmentDto>>(StatusCodes.Status400BadRequest);

            // 4. Get Diagnosis Types
            group.MapGet("/types", async ([FromServices] ISender sender, [FromQuery] int? clinicId) =>
            {
                var result = await sender.Send(new GetDiagnosisTypesQuery(clinicId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<DiagnosisTypeDto>>.Success(result.Value, "Diagnosis types retrieved."))
                    : Results.BadRequest(ApiResponse<List<DiagnosisTypeDto>>.Failure(result.Errors, "Failed to retrieve diagnosis types."));
            })
            .WithName("GetDiagnosisTypes")
            .WithSummary("Retrieves diagnosis types, optionally filtered by clinic ID")
            .Produces<ApiResponse<List<DiagnosisTypeDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<DiagnosisTypeDto>>>(StatusCodes.Status400BadRequest);

            // 5. Get Procedures
            group.MapGet("/procedures", async ([FromServices] ISender sender, [FromQuery] int? clinicId) =>
            {
                var result = await sender.Send(new GetProceduresQuery(clinicId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<ProcedureDto>>.Success(result.Value, "Procedures retrieved."))
                    : Results.BadRequest(ApiResponse<List<ProcedureDto>>.Failure(result.Errors, "Failed to retrieve procedures."));
            })
            .WithName("GetProcedures")
            .WithSummary("Retrieves procedures, optionally filtered by clinic ID")
            .Produces<ApiResponse<List<ProcedureDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<ProcedureDto>>>(StatusCodes.Status400BadRequest);
        }
    }
}