using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateUser;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.SetActiveTerm;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllConfigs;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllClinics;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllTerms;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetStudentRequirements;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetTermById;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetUserById;
using ZU_DCMS.APPLICATION.DTOs.Admin;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.Dashboard;

namespace ZU_DCMS.API.Endpoints.Admin
{
    /// <summary>
    /// Extension class to register Admin endpoints.
    /// </summary>
    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/admin")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Administration");

            // __ Management Dashboard (Dean / Vice Dean / Professor / Admin) __ //
            group.MapGet("/dashboard/daily-metrics", async ([FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetDailyMetricsQuery());
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<DailyMetricsDto>.Success(result.Value, "Daily metrics retrieved."))
                    : Results.BadRequest(ApiResponse<DailyMetricsDto>.Failure(result.Errors, "Failed to retrieve daily metrics."));
            })
            .RequireAuthorization("ManagementPolicy")
            .WithName("GetDailyMetrics")
            .WithSummary("Returns aggregated daily clinical statistics for management roles")
            .Produces<ApiResponse<DailyMetricsDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DailyMetricsDto>>(StatusCodes.Status400BadRequest);

            // ==== ADMIN ONLY QUERIES ====

            group.MapGet("/configs", async ([FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetAllConfigsQuery());
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<SystemConfigDto>>.Success(result.Value, "System configurations retrieved."))
                    : Results.BadRequest(ApiResponse<List<SystemConfigDto>>.Failure(result.Errors, "Failed to retrieve configurations."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetAllConfigs")
            .WithSummary("Retrieves all system configurations")
            .Produces<ApiResponse<List<SystemConfigDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<SystemConfigDto>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/clinics", async ([FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetAllClinicsQuery());
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<ClinicDto>>.Success(result.Value, "Clinics retrieved."))
                    : Results.BadRequest(ApiResponse<List<ClinicDto>>.Failure(result.Errors, "Failed to retrieve clinics."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetAllClinics")
            .WithSummary("Retrieves all clinics for requirement setting")
            .Produces<ApiResponse<List<ClinicDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<ClinicDto>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/terms", async ([FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetAllTermsQuery());
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<TermDto>>.Success(result.Value, "Terms retrieved."))
                    : Results.BadRequest(ApiResponse<List<TermDto>>.Failure(result.Errors, "Failed to retrieve terms."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetAllTerms")
            .WithSummary("Retrieves all academic terms")
            .Produces<ApiResponse<List<TermDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<TermDto>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/terms/{termId}", async ([FromServices] ISender sender, [AsParameters] GetTermByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<TermDto>.Success(result.Value, "Term retrieved."))
                    : Results.NotFound(ApiResponse<TermDto>.Failure(result.Error, "Term not found."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetTermById")
            .WithSummary("Retrieves a specific term by its ID")
            .Produces<ApiResponse<TermDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TermDto>>(StatusCodes.Status404NotFound);

            group.MapGet("/users", async ([AsParameters] PagedRequest request, [FromQuery] string? role, [FromServices] ISender sender) =>
            {
                var query = new GetAllUsersQuery(request, role ?? "");
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PagedResult<StaffUsersDto>>.Success(result.Value, "Users retrieved."))
                    : Results.BadRequest(ApiResponse<PagedResult<StaffUsersDto>>.Failure(result.Error, "Failed to retrieve users."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetAllUsers")
            .WithSummary("Retrieves all application users")
            .Produces<ApiResponse<PagedResult<StaffUsersDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PagedResult<StaffUsersDto>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/users/{userId}", async ([FromServices] ISender sender, [AsParameters] GetUserByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<StaffUsersDto>.Success(result.Value, "User retrieved."))
                    : Results.NotFound(ApiResponse<StaffUsersDto>.Failure(result.Error, "User not found."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetUserById")
            .WithSummary("Retrieves a specific user by their ApplicationBuilder ID")
            .Produces<ApiResponse<StaffUsersDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<StaffUsersDto>>(StatusCodes.Status404NotFound);

            group.MapGet("/students/{studentId}/requirements/term/{termId}", async ([FromRoute] int studentId, [FromRoute] int termId, [FromServices] ISender sender) =>
            {
                var query = new GetStudentRequirementsQuery(studentId, termId);
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<StudentRequirementDto>>.Success(result.Value, "Student requirements records retrieved."))
                    : Results.BadRequest(ApiResponse<List<StudentRequirementDto>>.Failure(result.Error, "Failed to retrieve requirements."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("GetAllStudentRequirements")
            .WithSummary("Retrieves all set requirements for students")
            .Produces<ApiResponse<List<StudentRequirementDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<StudentRequirementDto>>>(StatusCodes.Status400BadRequest);

            // ==== COMMANDS ====

            group.MapPost("/terms", async ([FromServices] ISender sender, [FromBody] CreateTermCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<TermDto>.Success(result.Value, "Term successfully created."))
                    : Results.BadRequest(ApiResponse<TermDto>.Failure(result.Error, "Failed to create term."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("CreateTerm")
            .WithSummary("Creates a new academic term")
            .Produces<ApiResponse<TermDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TermDto>>(StatusCodes.Status400BadRequest);

            group.MapPut("/terms", async ([FromServices] ISender sender, [FromBody] UpdateTermCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<TermDto>.Success(result.Value, "Term successfully updated."))
                    : Results.BadRequest(ApiResponse<TermDto>.Failure(result.Error, "Failed to update term."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("UpdateTerm")
            .WithSummary("Updates an existing term")
            .Produces<ApiResponse<TermDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TermDto>>(StatusCodes.Status400BadRequest);

            group.MapPut("/terms/set-active", async ([FromServices] ISender sender, [FromBody] SetActiveTermCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Active term updated."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Error, "Failed to set active term."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("SetActiveTerm")
            .WithSummary("Marks a specific academic term as active")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            group.MapPost("/users", async ([FromServices] ISender sender, [FromBody] CreateUserCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<StaffUsersDto>.Success(result.Value, "User created successfully."))
                    : Results.BadRequest(ApiResponse<StaffUsersDto>.Failure(result.Error, "Failed to create user."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("CreateUser")
            .WithSummary("Registers a new staff or student user directly")
            .Produces<ApiResponse<StaffUsersDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<StaffUsersDto>>(StatusCodes.Status400BadRequest);

            group.MapPut("/student-requirements", async ([FromServices] ISender sender, [FromBody] SetStudentRequirementsCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Student requirements successfully updated."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Error, "Failed to update requirements."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("SetStudentRequirements")
            .WithSummary("Sets or modifies the clinical requirements threshold for students")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            group.MapPut("/student-requirements/yearly", async ([FromServices] ISender sender, [FromBody] SetTermRequirementsCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Yearly requirements successfully updated."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Error, "Failed to update yearly requirements."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("SetYearlyRequirements")
            .WithSummary("Sets clinical requirements for all students in a specific academic year")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            group.MapPut("/configs", async ([FromServices] ISender sender, [FromBody] UpdateConfigCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "System configuration successfully updated."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Error, "Failed to update config."));
            })
            .RequireAuthorization("AdminPolicy")
            .WithName("UpdateConfig")
            .WithSummary("Updates a specific system operational configuration")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);
        }
    }
}