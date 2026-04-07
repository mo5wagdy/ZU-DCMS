using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Services.Implementations;
using ZU_DCMS.APPLICATION.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace ZU_DCMS.APPLICATION.Extentions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // __________ Validators __________ //
            services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

            // __________ AutoMapper __________ //
            services.AddAutoMapper(cfg => { }, typeof(ApplicationServiceExtensions));

            /* Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            services.AddScoped<ICaseService, CaseService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IPaymentService, PaymentService>();
            */
            return services;
        }
    }
}
