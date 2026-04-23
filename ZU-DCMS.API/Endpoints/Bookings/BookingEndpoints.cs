using Asp.Versioning.Builder;
using MediatR;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.CancelBooking;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.CreateBooking;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.PostponeBooking;
using ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetPatientBookings;

namespace ZU_DCMS.API.Endpoints.Bookings
{
    /// <summary>
    /// Extension class to register Booking operations endpoints.
    /// </summary>
    public static class BookingEndpoints
    {
        public static void MapBookingEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/bookings")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Bookings");

            // 1. Create Booking
            group.MapPost("/", async (ISender sender, CreateBookingCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Booking successfully created."));
            })
            .RequireAuthorization("PatientPolicy") // Security check ensuring only patients book for themselves
            .WithName("CreateBooking")
            .WithSummary("Creates a new booking reservation for the authenticated patient");

            // 2. Cancel Booking
            group.MapPut("/cancel", async (ISender sender, CancelBookingCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Booking successfully cancelled."));
            })
            .RequireAuthorization("PatientPolicy")
            .WithName("CancelBooking")
            .WithSummary("Cancels an existing booking for the authenticated patient");

            // 3. Postpone Booking
            group.MapPut("/postpone", async (ISender sender, PostponeBookingCommand command) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(ApiResponse<object>.Success(result, "Booking successfully postponed."));
            })
            .RequireAuthorization("PatientPolicy")
            .WithName("PostponeBooking")
            .WithSummary("Postpones an active booking to a different timeframe or slot");

            // 4. Retrieve My Bookings
            group.MapGet("/patient", async (ISender sender, [AsParameters] GetPatientBookingsQuery query) =>
            {
                var result = await sender.Send(query);
                return Results.Ok(ApiResponse<object>.Success(result, "Patient bookings retrieved."));
            })
            .RequireAuthorization("PatientPolicy")
            .WithName("GetPatientBookings")
            .WithSummary("Retrieves all bookings explicitly mapping to the authenticated patient");
        }
    }
}
