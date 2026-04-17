using FluentValidation;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZU_DCMS.Application.Services.Implementations;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Handlers;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Handlers;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Handlers;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Handlers;
using ZU_DCMS.APPLICATION.Services.Implementations;
using ZU_DCMS.APPLICATION.Services.Interfaces;

namespace ZU_DCMS.APPLICATION.Extentions
{
    public static class ApplicationServiceExtensions
    {
        // ___________________________ Application Service Registrations ___________________________ //
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // __________ Validators __________ //
            services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

            // __________ AutoMapper __________ //
            services.AddAutoMapper(cfg => { }, typeof(ApplicationServiceExtensions));

            // __________ Event Handlers __________ //
            services.AddScoped<IEventPublisher, HangfireEventPublisher>();
            services.AddScoped<EventDispatcher>();

            // __________ Booking Events __________ //
            services.AddScoped<IEventHandler<BookingCreatedEvent>, BookingCreatedHandler>();
            services.AddScoped<IEventHandler<BookingCancelledEvent>, BookingCancelledHandler>();
            services.AddScoped<IEventHandler<BookingPostponedEvent>, BookingPostponedHandler>();

            // __________ Payment Events __________ //
            services.AddScoped<IEventHandler<PaymentCompletedEvent>, PaymentCompletedHandler>();

            // __________ Case Events __________ //
            services.AddScoped<IEventHandler<CaseAssignedEvent>, CaseAssignedHandler>();

            // _________ Diagnosis Events __________ //
            services.AddScoped<IEventHandler<DiagnosisCreatedEvent>, DiagnosisCreatedHandler>();

            // __________ Service Registrations __________ //
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IAdminService, AdminService>();
            /* Services
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IPaymentService, PaymentService>();
            */
            return services;
        }
    }
}
