using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetAllStudents;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetRequirements;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentById;
using ZU_DCMS.APPLICATION.Features.Students.Queries.GetStudentByUserId;

namespace ZU_DCMS.API.Endpoints.Students
{
    /// <summary>
    /// Extension class to register Student endpoints.
    /// </summary>
    public static class StudentEndpoints
    {
        public static void MapStudentEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/students")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Students");

            // 1. Get All Students
            group.MapGet("/", async ([AsParameters] PagedRequest request, ISender sender) =>
            {
                var query = new GetAllStudentsQuery(request);
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "All students retrieved."));
            })
            // Policy allowing Staff and High-Level Roles (Dean, ViceDean, Professor, Admin)
            .RequireAuthorization("StaffViewPolicy") 
            .WithName("GetAllStudents")
            .WithSummary("Retrieves a list of all students");

            // 2. Get Student By Intern/Student ID
            group.MapGet("/{id}", async (ISender sender, [AsParameters] GetStudentByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Student details retrieved."));
            })
            .RequireAuthorization("StaffViewPolicy")
            .WithName("GetStudentById")
            .WithSummary("Retrieves a specific student by their specific DB ID");

            // 3. Get Student By Identity User ID
            group.MapGet("/user/{id}", async (ISender sender, [AsParameters] GetStudentByUserIdQuery query) =>
            {
                // Can be accessed by Staff or the student themselves.
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Student details by user ID retrieved."));
            })
            .RequireAuthorization() 
            .WithName("GetStudentByUserId")
            .WithSummary("Retrieves a specific student based on their Identity User ID");

            // 4. Get Student Requirements
            group.MapGet("/requirements", async (ISender sender, [AsParameters] GetRequirementsQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Student requirements retrieved."));
            })
            // Must be authenticated. Usually, they retrieve their own.
            .RequireAuthorization() 
            .WithName("GetStudentRequirements")
            .WithSummary("Retrieves the clinical requirements assigned to a given student");
        }
    }
}
