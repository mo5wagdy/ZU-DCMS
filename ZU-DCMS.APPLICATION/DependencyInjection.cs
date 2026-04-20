using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Booking.Handlers;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Case.Handlers;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Diagnosis.Handlers;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Events;
using ZU_DCMS.APPLICATION.Background_Jobs.Features.Payment.Handlers;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.Token;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.Login;

namespace ZU_DCMS.APPLICATION
{
    public static class DependencyInjection
    {
        // ___________________________ Application Service Registrations ___________________________ //
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // __________ Validators __________ //
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // __________ AutoMapper __________ //
            services.AddAutoMapper(cfg => { }, typeof(DependencyInjection));

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

            // __________ Diagnosis Events ___________ //
            services.AddScoped<IEventHandler<DiagnosisCreatedEvent>, DiagnosisCreatedHandler>();

            // __________ MediatR __________ //
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginHandler).Assembly));

            // __________ Caching Registerations __________ // 
            services.AddFusionCacheStackExchangeRedisBackplane(o =>
            {
                o.Configuration = "localhost:6379";
            });

            services.AddFusionCacheSystemTextJsonSerializer();

            services.AddFusionCache()
                    .WithDefaultEntryOptions(new FusionCacheEntryOptions
                    {
                        Duration = CacheDuration.Short,
                        IsFailSafeEnabled = true,
                        FailSafeMaxDuration = CacheDuration.Medium
                    })
                    .WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>(), new FusionCacheSystemTextJsonSerializer());

            // __________ Common Services Registrations __________ //
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}