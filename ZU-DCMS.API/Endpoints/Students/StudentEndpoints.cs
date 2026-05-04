using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetAllStudents;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetRequirements;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentById;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentByUserId;
using ZU_DCMS.APPLICATION.DTOs.Student;

namespace ZU_DCMS.API.Endpoints.Students
{
    /// <summary>
    /// Extension class to register Student endpoints.
    /// </summary>
    public static class StudentEndpoints
    {
        public static void MapStudentEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/students")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Students");

            // 1. Get All Students
            group.MapGet("/", async ([AsParameters] PagedRequest request, [FromQuery] int? academicYear, [FromServices] ISender sender) =>
            {
                var query = new GetAllStudentsQuery(request, academicYear);
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PagedResult<StudentDto>>.Success(result.Value, "All students retrieved."))
                    : Results.BadRequest(ApiResponse<PagedResult<StudentDto>>.Failure(result.Errors, "Failed to retrieve students."));
            })
            // Policy allowing Staff and High-Level Roles (Dean, ViceDean, Professor, Admin)
            .RequireAuthorization("StaffViewPolicy") 
            .WithName("GetAllStudents")
            .WithSummary("Retrieves a list of all students")
            .Produces<ApiResponse<PagedResult<StudentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PagedResult<StudentDto>>>(StatusCodes.Status400BadRequest);

            // 2. Get Student By Intern/Student ID
            group.MapGet("/{studentId}", async ([FromServices] ISender sender, [AsParameters] GetStudentByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<StudentDto>.Success(result.Value, "Student details retrieved."))
                    : Results.NotFound(ApiResponse<StudentDto>.Failure(result.Errors, "Student not found."));
            })
            .RequireAuthorization("StaffViewPolicy")
            .WithName("GetStudentById")
            .WithSummary("Retrieves a specific student by their specific DB ID")
            .Produces<ApiResponse<StudentDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<StudentDto>>(StatusCodes.Status404NotFound);

            // 3. Get Student By Identity User ID
            group.MapGet("/user/{userId}", async ([FromServices] ISender sender, [AsParameters] GetStudentByUserIdQuery query) =>
            {
                // Can be accessed by Staff or the student themselves.
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<StudentDto>.Success(result.Value, "Student details by user ID retrieved."))
                    : Results.NotFound(ApiResponse<StudentDto>.Failure(result.Errors, "Student not found by given user ID."));
            })
            .RequireAuthorization() 
            .WithName("GetStudentByUserId")
            .WithSummary("Retrieves a specific student based on their Identity User ID")
            .Produces<ApiResponse<StudentDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<StudentDto>>(StatusCodes.Status404NotFound);

            // 4. Get Student Requirements
            group.MapGet("/requirements", async ([FromServices] ISender sender, [AsParameters] GetRequirementsQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<StudentRequirementDto>>.Success(result.Value, "Student requirements retrieved."))
                    : Results.BadRequest(ApiResponse<List<StudentRequirementDto>>.Failure(result.Errors, "Failed to retrieve student requirements."));
            })
            // Must be authenticated. Usually, they retrieve their own.
            .RequireAuthorization() 
            .WithName("GetStudentRequirements")
            .WithSummary("Retrieves the clinical requirements assigned to a given student")
            .Produces<ApiResponse<List<StudentRequirementDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<StudentRequirementDto>>>(StatusCodes.Status400BadRequest);
        }
    }
}
