using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Hangfire;
using ZU_DCMS.APPLICATION;
using ZU_DCMS.INFRASTRUCTURE;
using ZU_DCMS.API.Middlewares; // Import middleware
using ZU_DCMS.API.Endpoints.Admin;
using ZU_DCMS.API.Endpoints.Auth;
using ZU_DCMS.API.Endpoints.Students;
using ZU_DCMS.API.Endpoints.Diagnosis;
using ZU_DCMS.API.Endpoints.Sessions;
using ZU_DCMS.API.Endpoints.Patients;
using ZU_DCMS.API.Endpoints.Bookings;
using ZU_DCMS.API.Endpoints.Cases;
using ZU_DCMS.API.Endpoints.Lookups;

namespace ZU_DCMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // 1. Add infrastructure services
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // 2. Add application services
            builder.Services.AddApplicationServices();

            // 3. Add centralized API services (Auth, Versioning, Swagger, CORS, RateLimiting)
            builder.Services.AddApiServices(builder.Configuration);

            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // 4. Configure JSON serialization to handle circular references safely
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            // Add custom exception middleware early in the pipeline
            app.UseMiddleware<CustomExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                
                // Enable Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZU-DCMS API V1");
                });
            }

            app.UseHttpsRedirection();

            // Use CORS policy
            app.UseCors("ZU-DCMS-React");

            // Use Rate Limiting
            app.UseRateLimiter();

            // Use Authentication (Must be before Authorization)
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire");

            app.MapControllers();

            // Set up Global API Versioning for Minimal APIs
            var apiVersionSet = app.NewApiVersionSet()
                                   .HasApiVersion(new ApiVersion(1, 0))
                                   .ReportApiVersions()
                                   .Build();

            // Register Grouped Feature Endpoints
            app.MapAuthEndpoints(apiVersionSet);
            app.MapAdminEndpoints(apiVersionSet);
            app.MapStudentEndpoints(apiVersionSet);
            app.MapDiagnosisEndpoints(apiVersionSet);
            app.MapSessionEndpoints(apiVersionSet);
            app.MapPatientEndpoints(apiVersionSet);
            app.MapBookingEndpoints(apiVersionSet);
            app.MapCaseEndpoints(apiVersionSet);
            app.MapLookupEndpoints(apiVersionSet);

            app.Run();
        }
    }
}