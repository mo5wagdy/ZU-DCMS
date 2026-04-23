using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewCase;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.SubmitCaseForReview;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCaseById;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentProgress;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCasesForReview;

namespace ZU_DCMS.API.Endpoints.Cases
{
    /// <summary>
    /// Extension class to register Clinical Cases submission and evaluation workflows.
    /// </summary>
    public static class CaseEndpoints
    {
        public static void MapCaseEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/cases")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Clinical Cases");

            // ==== STUDENT ACTIONS ====

            // 1. Add procedural progress into a clinic session iteration
            group.MapPost("/progress", async (ISender sender, AddSessionProgressCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Session case progress strictly logged."));
            })
            .RequireAuthorization("StudentPolicy") // Prevents anyone else from messing with the sequence
            .WithName("AddSessionProgress")
            .WithSummary("Records iterative steps/progress metrics for an ongoing clinical case operation");

            // 2. Wrap up case structure and send it up for supervisor grading
            group.MapPost("/submit", async (ISender sender, SubmitCaseForReviewCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Case successfully submitted for formal review."));
            })
            .RequireAuthorization("StudentPolicy")
            .WithName("SubmitCaseForReview")
            .WithSummary("Ends the workflow sequence and pushes the case to the pending evaluator review queue");

            // 3. Status Check
            group.MapGet("/progress", async (ISender sender, [AsParameters] GetStudentProgressQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Student progress stats parsed perfectly."));
            })
            .RequireAuthorization("StudentPolicy")
            .WithName("GetStudentProgress")
            .WithSummary("Retrieves the general clinical progress overview constraints for the authorized student");


            // ==== STAFF / INSTRUCTOR ACTIONS ====

            // 4. Submit an instructor grade/decision on a student's case
            group.MapPost("/review", async (ISender sender, ReviewCaseCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "The system registered the formal review evaluation securely."));
            })
            .RequireAuthorization("StaffReviewPolicy") // Only for High level staff: Instructors, Dean, ViceDean, Professor Role structures
            .WithName("ReviewCase")
            .WithSummary("Registers a formal grade and detailed review footprint for a submitted student case");

            // 5. Look up queue
            group.MapGet("/pending-reviews", async (ISender sender, [AsParameters] GetCasesForReviewQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Incoming queue of submission cases successfully mapped."));
            })
            .RequireAuthorization("StaffReviewPolicy")
            .WithName("GetCasesForReview")
            .WithSummary("Retrieves all submitted procedural cases currently pending instructor validation");

            // ==== COMMON LOOKUP ====
            // 6. Common entity fallback
            group.MapGet("/{id}", async (ISender sender, [AsParameters] GetCaseByIdQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Extracted the detailed case structure blueprint."));
            })
            .RequireAuthorization() // Authorized structure allows owner tracking or staff inspection internally mapped
            .WithName("GetCaseById")
            .WithSummary("Retrieves the full object framework for a specific target clinical case");
        }
    }
}
