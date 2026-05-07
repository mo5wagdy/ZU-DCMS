using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;
using ZU_DCMS.APPLICATION.Background_Jobs.Events;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Common.MediatR_Behaviors;
using ZU_DCMS.APPLICATION.Common.Token;
using ZU_DCMS.APPLICATION.Features.Auth.Commands.Login;
using ZU_DCMS.APPLICATION.Contracts.Engine;
using ZU_DCMS.APPLICATION.Features.Diagnosis.Engine;

namespace ZU_DCMS.APPLICATION
{
    public static class DependencyInjection
    {
        // ___________________________ Application Service Registrations ___________________________ //
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // __________ MediatR __________ //
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginHandler).Assembly));

            // __________ MediatR behaviors __________ //
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // __________ Validators __________ //
            services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

            // __________ AutoMapper __________ //
            services.AddAutoMapper(cfg => {}, AppDomain.CurrentDomain.GetAssemblies());

            // __________ Event Handlers __________ //
            services.AddScoped<IEventPublisher, HangfireEventPublisher>();
            services.AddScoped<EventDispatcher>();

            //// __________ Booking Events __________ //
            //services.AddScoped<IEventHandler<BookingCreatedEvent>, BookingCreatedHandler>();
            //services.AddScoped<IEventHandler<BookingCancelledEvent>, BookingCancelledHandler>();
            //services.AddScoped<IEventHandler<BookingPostponedEvent>, BookingPostponedHandler>();

            //// __________ Payment Events __________ //
            //services.AddScoped<IEventHandler<PaymentCompletedEvent>, PaymentCompletedHandler>();

            //// __________ Case Events __________ //
            //services.AddScoped<IEventHandler<CaseAssignedEvent>, CaseAssignedHandler>();

            //// __________ Diagnosis Events ___________ //
            //services.AddScoped<IEventHandler<DiagnosisCreatedEvent>, DiagnosisCreatedHandler>();


            // __________ Caching Registerations __________ // 
            services.AddMemoryCache();

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
                    });
                    //.WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>(), new FusionCacheSystemTextJsonSerializer());

            // __________ Common Services Registrations __________ //
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAutoAssignmentEngine, AutoAssignmentEngine>();

            return services;
        }
    }
}