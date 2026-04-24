using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZU_DCMS.API.Common;
using ZU_DCMS.APPLICATION.Common.Pagination;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.CancelBooking;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.CreateBooking;
using ZU_DCMS.APPLICATION.Features.Bookings.Commands.PostponeBooking;
using ZU_DCMS.APPLICATION.Features.Bookings.Queries.GetPatientBookings;
using ZU_DCMS.APPLICATION.DTOs.Booking;

namespace ZU_DCMS.API.Endpoints.Bookings
{
    /// <summary>
    /// Extension class to register Booking operations endpoints.
    /// </summary>
    public static class BookingEndpoints
    {
        public static void MapBookingEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("api/v1/bookings")
                           .WithApiVersionSet(versionSet)
                           .WithTags("Bookings");

            // 1. Create Booking
            group.MapPost("/", async ([FromServices] ISender sender, [FromBody] CreateBookingCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<BookingDto>.Success(result.Value, "Booking successfully created."))
                    : Results.BadRequest(ApiResponse<BookingDto>.Failure(result.Errors, "Failed to create booking."));
            })
            .RequireAuthorization("PatientPolicy") // Security check ensuring only patients book for themselves
            .WithName("CreateBooking")
            .WithSummary("Creates a new booking reservation for the authenticated patient")
            .Produces<ApiResponse<BookingDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BookingDto>>(StatusCodes.Status400BadRequest);

            // 2. Cancel Booking
            group.MapPut("/cancel", async ([FromServices] ISender sender, [FromBody] CancelBookingCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Booking successfully cancelled."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Errors, "Failed to cancel booking."));
            })
            .RequireAuthorization("PatientPolicy")
            .WithName("CancelBooking")
            .WithSummary("Cancels an existing booking for the authenticated patient")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            // 3. Postpone Booking
            group.MapPut("/postpone", async ([FromServices] ISender sender, [FromBody] PostponeBookingCommand command) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<string>.Success(string.Empty, "Booking successfully postponed."))
                    : Results.BadRequest(ApiResponse<string>.Failure(result.Errors, "Failed to postpone booking."));
            })
            .RequireAuthorization("PatientPolicy")
            .WithName("PostponeBooking")
            .WithSummary("Postpones an active booking to a different timeframe or slot")
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            // 4. Retrieve My Bookings
            group.MapGet("/patient", async ([FromQuery] int patientId, [AsParameters] PagedRequest request, [FromServices] ISender sender) =>
            {
                var query = new GetPatientBookingsQuery(patientId, request);
                var result = await sender.Send(query);
                return result.IsSuccess
                    ? Results.Ok(ApiResponse<PagedResult<BookingDto>>.Success(result.Value, "Patient bookings retrieved."))
                    : Results.BadRequest(ApiResponse<PagedResult<BookingDto>>.Failure(result.Errors, "Failed to retrieve bookings."));
            })
            .RequireAuthorization("PatientPolicy")
            .WithName("GetPatientBookings")
            .WithSummary("Retrieves all bookings explicitly mapping to the authenticated patient")
            .Produces<ApiResponse<PagedResult<BookingDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PagedResult<BookingDto>>>(StatusCodes.Status400BadRequest);
        }
    }
}
