using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateTerm;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.CreateUser;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.SetActiveTerm;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.SetStudentRequirements;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateConfig;
using ZU_DCMS.APPLICATION.Features.Admin.Commands.UpdateTerm;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllConfigs;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllTerms;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetAllUsers;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetStudentRequirements;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetTermById;
using ZU_DCMS.APPLICATION.Features.Admin.Queries.GetUserById;

namespace ZU_DCMS.API.Endpoints.Admin
{
    /// <summary>
    /// Extension class to register Admin endpoints.
    /// </summary>
    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/admin")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Administration")
                           .RequireAuthorization("AdminPolicy"); // Restrict all these to the Admin policy

            // ==== QUERIES ====

            group.MapGet("/configs", async (ISender sender) =>
            {
                var result = await sender.Send(new GetAllConfigsQuery());
                return Results.Ok(ApiResponse<object>.Success(result, "System configurations retrieved."));
            })
            .WithName("GetAllConfigs")
            .WithSummary("Retrieves all system configurations");

            group.MapGet("/terms", async (ISender sender) =>
            {
                var result = await sender.Send(new GetAllTermsQuery());
                return Results.Ok(ApiResponse<object>.Success(result, "Terms retrieved."));
            })
            .WithName("GetAllTerms")
            .WithSummary("Retrieves all academic terms");

            group.MapGet("/terms/{id}", async (ISender sender, [AsParameters] GetTermByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Term retrieved."));
            })
            .WithName("GetTermById")
            .WithSummary("Retrieves a specific term by its ID");

            group.MapGet("/users", async ([AsParameters] PagedRequest request, string? search, ISender sender) =>
            {
                var query = new GetAllUsersQuery(request, search ?? "");
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Users retrieved."));
            })
            .WithName("GetAllUsers")
            .WithSummary("Retrieves all application users");

            group.MapGet("/users/{id}", async (ISender sender, [AsParameters] GetUserByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "User retrieved."));
            })
            .WithName("GetUserById")
            .WithSummary("Retrieves a specific user by their ApplicationBuilder ID");

            group.MapGet("/students/{studentId}/requirements/term/{termId}", async (int studentId, int termId, ISender sender) =>
            {
                var query = new GetStudentRequirementsQuery(studentId, termId);
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Student requirements records retrieved."));
            })
            .WithName("GetAllStudentRequirements")
            .WithSummary("Retrieves all set requirements for students");

            // ==== COMMANDS ====

            group.MapPost("/terms", async (ISender sender, CreateTermCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Term successfully created."));
            })
            .WithName("CreateTerm")
            .WithSummary("Creates a new academic term");

            group.MapPut("/terms", async (ISender sender, UpdateTermCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Term successfully updated."));
            })
            .WithName("UpdateTerm")
            .WithSummary("Updates an existing term");

            group.MapPut("/terms/set-active", async (ISender sender, SetActiveTermCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Active term updated."));
            })
            .WithName("SetActiveTerm")
            .WithSummary("Marks a specific academic term as active");

            group.MapPost("/users", async (ISender sender, CreateUserCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "User created successfully."));
            })
            .WithName("CreateUser")
            .WithSummary("Registers a new staff or student user directly");

            group.MapPut("/student-requirements", async (ISender sender, SetStudentRequirementsCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Student requirements successfully updated."));
            })
            .WithName("SetStudentRequirements")
            .WithSummary("Sets or modifies the clinical requirements threshold for students");

            group.MapPut("/configs", async (ISender sender, UpdateConfigCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "System configuration successfully updated."));
            })
            .WithName("UpdateConfig")
            .WithSummary("Updates a specific system operational configuration");
        }
    }
}
