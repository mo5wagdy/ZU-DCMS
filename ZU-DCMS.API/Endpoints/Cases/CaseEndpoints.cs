using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.DTOs.Case;
using ZU_DCMS.APPLICATION.DTOs.Student;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.AddSessionProgress;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewCase;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.ReviewAssignment;
using ZU_DCMS.APPLICATION.Features.Cases.Commands.SubmitCaseForReview;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCaseById;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCaseReviews;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetCasesForReview;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetPendingAssignments;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetReviewedAssignments;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetReviewedCases;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentCases;
using ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentProgress;

namespace ZU_DCMS.API.Endpoints.Cases
{
    /// <summary>
    /// Extension class to register Clinical Cases submission and evaluation workflows.
    /// </summary>
    public static class CaseEndpoints
    {
        public static void MapCaseEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/cases")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Clinical Cases");

            // ==== STUDENT ACTIONS ====

            // 1. Add procedural progress into a clinic session iteration
            group.MapPost("/progress", async ([FromServices] ISender sender, [FromBody] AddSessionProgressCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<CaseSessionDto>.Success(result.Value, "Session case progress strictly logged."))
                    : Results.BadRequest(ApiResponse<CaseSessionDto>.Failure(result.Errors, "Failed to log case progress."));
            })
            .RequireAuthorization("StudentPolicy") // Prevents anyone else from messing with the sequence
            .WithName("AddSessionProgress")
            .WithSummary("Records iterative steps/progress metrics for an ongoing clinical case operation")
            .Produces<ApiResponse<CaseSessionDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CaseSessionDto>>(StatusCodes.Status400BadRequest);

            // 2. Wrap up case structure and send it up for supervisor grading
            group.MapPost("/submit", async ([FromServices] ISender sender, [FromBody] SubmitCaseForReviewCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Case successfully submitted for formal review."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Errors, "Failed to submit case."));
            })
            .RequireAuthorization("StudentPolicy")
            .WithName("SubmitCaseForReview")
            .WithSummary("Ends the workflow sequence and pushes the case to the pending evaluator review queue")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            // 3. Status Check
            group.MapGet("/progress", async ([FromServices] ISender sender, [AsParameters] GetStudentProgressQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<StudentProgressDto>.Success(result.Value, "Student progress stats parsed perfectly."))
                    : Results.BadRequest(ApiResponse<StudentProgressDto>.Failure(result.Errors, "Failed to get progress stats."));
            })
            .RequireAuthorization("StaffCaseAccessPolicy")
            .WithName("GetStudentProgress")
            .WithSummary("Retrieves the general clinical progress overview constraints for the authorized student")
            .Produces<ApiResponse<StudentProgressDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<StudentProgressDto>>(StatusCodes.Status400BadRequest);

            group.MapGet("/today-patients", async ([FromServices] ISender sender, string userId) =>
            {
                var query = new ZU_DCMS.APPLICATION.Features.Cases.Queries.GetStudentTodayPatients.GetStudentTodayPatientsQuery(userId);
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<CaseAssignmentDto>>.Success(result.Value, "Today's patients retrieved."))
                    : Results.BadRequest(ApiResponse<List<CaseAssignmentDto>>.Failure(result.Errors, "Failed to retrieve today's patients."));
            })
            .RequireAuthorization("StaffCaseAccessPolicy")
            .WithName("GetStudentTodayPatients")
            .WithSummary("Retrieves assigned cases that have a confirmed booking for today")
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status400BadRequest);



            // ==== STAFF / INSTRUCTOR ACTIONS ====

            // 4. Submit an Teaching assistant grade/decision on a student's case
            group.MapPost("/review", async ([FromServices] ISender sender, [FromBody] ReviewCaseCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "The system registered the formal review evaluation securely."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Errors, "Failed to register review evaluation."));
            })
            .RequireAuthorization("StaffReviewPolicy") // Only for High level staff: Instructors, Dean, ViceDean, Professor Role structures
            .WithName("ReviewCase")
            .WithSummary("Registers a formal grade and detailed review footprint for a submitted student case")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            // 5. Look up queue
            group.MapGet("/pending-reviews", async ([FromServices] ISender sender) =>
            {
                var query = new GetCasesForReviewQuery();
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<CaseAssignmentDto>>.Success(result.Value, "Incoming queue of submission cases successfully mapped."))
                    : Results.BadRequest(ApiResponse<List<CaseAssignmentDto>>.Failure(result.Errors, "Failed to map submission cases."));
            })
            .RequireAuthorization("StaffReviewPolicy")
            .WithName("GetCasesForReview")
            .WithSummary("Retrieves all submitted procedural cases currently pending instructor validation")
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status400BadRequest);

            // 5.1 Reviewed cases history (cases a TA has already approved or rejected)
            group.MapGet("/reviewed-by/{taUserId}", async ([FromServices] ISender sender, string taUserId) =>
            {
                var result = await sender.Send(new GetReviewedCasesQuery(taUserId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<CaseAssignmentDto>>.Success(result.Value, "Reviewed cases retrieved successfully."))
                    : Results.BadRequest(ApiResponse<List<CaseAssignmentDto>>.Failure(result.Errors, "Failed to retrieve reviewed cases."));
            })
            .RequireAuthorization("StaffReviewPolicy")
            .WithName("GetReviewedCases")
            .WithSummary("Returns all cases that a specific Teaching Assistant has already reviewed")
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status400BadRequest);

            // ==== PRE-TREATMENT ASSIGNMENT REVIEWS ====
            
            // 5.2 Look up queue for assignments pending TA approval
            group.MapGet("/pending-assignments", async ([FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetPendingAssignmentsQuery());
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<CaseAssignmentDto>>.Success(result.Value, "Pending assignments mapped."))
                    : Results.BadRequest(ApiResponse<List<CaseAssignmentDto>>.Failure(result.Errors, "Failed to map pending assignments."));
            })
            .RequireAuthorization("StaffReviewPolicy")
            .WithName("GetPendingAssignments")
            .WithSummary("Retrieves all auto-assigned cases currently pending instructor approval to start treatment")
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status400BadRequest);

            // 5.3 Submit a TA decision on a pending assignment
            group.MapPost("/assignment-review", async ([FromServices] ISender sender, [FromBody] ReviewAssignmentCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Assignment review processed successfully."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Errors, "Failed to process assignment review."));
            })
            .RequireAuthorization("StaffReviewPolicy")
            .WithName("ReviewAssignment")
            .WithSummary("Approves, escalates, or transfers a pending auto-assigned student case")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            // 5.4 Reviewed assignments history (assignments a TA has already approved, escalated, or transferred)
            group.MapGet("/reviewed-assignments/{taUserId}", async ([FromServices] ISender sender, string taUserId) =>
            {
                var result = await sender.Send(new GetReviewedAssignmentsQuery(taUserId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<CaseAssignmentDto>>.Success(result.Value, "Reviewed assignments retrieved successfully."))
                    : Results.BadRequest(ApiResponse<List<CaseAssignmentDto>>.Failure(result.Errors, "Failed to retrieve reviewed assignments."));
            })
            .RequireAuthorization("StaffReviewPolicy")
            .WithName("GetReviewedAssignments")
            .WithSummary("Returns all assignments that a specific Teaching Assistant has already reviewed")
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status400BadRequest);

            // ==== COMMON LOOKUP ====
            // 6. Common entity fallback
            group.MapGet("/{caseAssignmentId:int}", async ([FromServices] ISender sender, int caseAssignmentId) =>
            {
                var result = await sender.Send(new GetCaseByIdQuery(caseAssignmentId));
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<CaseAssignmentDto>.Success(result.Value, "Extracted the detailed case structure blueprint."))
                    : Results.NotFound(ApiResponse<CaseAssignmentDto>.Failure(result.Error, "Case not found."));
            })
            .RequireAuthorization("StaffCaseAccessPolicy") // Authorized structure allows owner tracking or staff inspection internally mapped
            .WithName("GetCaseById")
            .WithSummary("Retrieves the full object framework for a specific target clinical case")
            .Produces<ApiResponse<CaseAssignmentDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CaseAssignmentDto>>(StatusCodes.Status404NotFound);
            
            // 7. Student cases
            group.MapGet("/student/{studentId}", async ([FromServices] ISender sender, [AsParameters] GetStudentCasesQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<CaseAssignmentDto>>.Success(result.Value, "Retrived Student Cases Successfully"))
                    : Results.NotFound(ApiResponse<List<CaseAssignmentDto>>.Failure(result.Errors, "Student Cases not found"));
            })
            .RequireAuthorization("StaffCaseAccessPolicy") // Authorized structure allows owner tracking or staff inspection internally mapped
            .WithName("GetStudentCases")
            .WithSummary("Retrieves all student cases")
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CaseAssignmentDto>>>(StatusCodes.Status404NotFound);
            
            // 8. Student cases
            group.MapGet("/reviews", async ([FromServices] ISender sender, [AsParameters] GetCaseReviewsQuery query) =>
            {
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<List<ReviewCaseDto>>.Success(result.Value, "Retrived All Cases Reviews Successfully"))
                    : Results.NotFound(ApiResponse<List<ReviewCaseDto>>.Failure(result.Errors, "Cases Reviews Not Found"));
            })
            .RequireAuthorization("StaffCaseAccessPolicy") // Authorized structure allows owner tracking or staff inspection internally mapped
            .WithName("GetCasesReviews")
            .WithSummary("Retrieves all cases reviews")
            .Produces<ApiResponse<List<ReviewCaseDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<ReviewCaseDto>>>(StatusCodes.Status404NotFound);
        }
    }
}
