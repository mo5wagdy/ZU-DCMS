using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Sessions.Commands.GenerateSessions;
using ZU_DCMS.APPLICATION.Features.Sessions.Queries.FindSession;
using ZU_DCMS.APPLICATION.Features.Sessions.Queries.GetAvailableSlots;
using ZU_DCMS.APPLICATION.Features.Sessions.Queries.IsSessionAvailable;

namespace ZU_DCMS.API.Endpoints.Sessions
{
    /// <summary>
    /// Extension class to register Session and Clinic scheduling endpoints.
    /// </summary>
    public static class SessionEndpoints
    {
        public static void MapSessionEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/sessions")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Sessions");

            // 1. Check open slots (For Patients or Public booking access)
            group.MapGet("/available-slots", async (ISender sender, [AsParameters] GetAvailableSlotsQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Available slots retrieved."));
            })
            .RequireAuthorization("PublicViewPolicy") // Patient/Public roles
            .WithName("GetAvailableSlots")
            .WithSummary("Retrieves open clinic session slots within a timeframe");

            // 2. Staff lookup session filters
            group.MapGet("/find", async (ISender sender, [AsParameters] FindSessionQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Session records retrieved."));
            })
            .RequireAuthorization("StaffViewPolicy") // Internal doctors, admins, receptionists
            .WithName("FindSession")
            .WithSummary("Searches for specific clinical sessions via complex filters");

            // 3. Logic check for session capacity/flags
            group.MapGet("/check-availability", async (ISender sender, [AsParameters] IsSessionAvailableQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Availability evaluated."));
            })
            .RequireAuthorization("PublicViewPolicy")
            .WithName("IsSessionAvailable")
            .WithSummary("Verifies precisely if a specific session can support more bookings");

            // 4. Generator (Admin Only)
            group.MapPost("/generate", async (ISender sender, GenerateSessionsCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Sessions formally generated."));
            })
            .RequireAuthorization("AdminPolicy") // strictly Admin generator
            .WithName("GenerateSessions")
            .WithSummary("Bulk generates routine clinic sessions in advance for the layout");
        }
    }
}
