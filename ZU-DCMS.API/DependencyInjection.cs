using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;

namespace ZU_DCMS.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // __ Configure Authentication & JWT Settings __ //
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing from configuration.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            // __ Authorizing Policies __ //
            services.AddAuthorization(o =>
            {
                o.AddPolicy("PatientPolicy", policy => policy.RequireClaim("UserType", "Patient").RequireRole("Patient"));
                o.AddPolicy("AdminPolicy", policy => policy.RequireClaim("UserType", "Staff").RequireRole("Admin"));
                o.AddPolicy("StudentPolicy", policy => policy.RequireClaim("UserType", "Staff").RequireRole("Student"));
                o.AddPolicy("StaffReviewPolicy", policy => policy.RequireClaim("UserType", "Staff").RequireRole("TeachingAssistant", "Dean", "ViceDean", "Professor", "Admin"));
                o.AddPolicy("StaffCaseAccessPolicy", policy => policy.RequireClaim("UserType", "Staff").RequireRole("Student", "TeachingAssistant", "Dean", "ViceDean", "Professor", "Admin"));
                o.AddPolicy("ClinicalCorePolicy", policy => policy.RequireClaim("UserType", "Staff").RequireRole("InternDoctor", "Admin"));
                o.AddPolicy("PublicViewPolicy", policy => policy.RequireAssertion(context => 
                    context.User.HasClaim("UserType", "Staff") || context.User.HasClaim("UserType", "Patient")));
                o.AddPolicy("StaffViewPolicy", policy => policy.RequireClaim("UserType", "Staff").RequireRole("InternDoctor", "TeachingAssistant", "Dean", "ViceDean", "Professor", "Admin"));
            });

            // __ Configure API Versioning (Defaults to V1) __ //
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0); // V1
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            // __ Configure Swagger with JWT Support __ //
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZU-DCMS API", Version = "v1" });

                // Define the security scheme for Swagger UI
                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization. Example: 'bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                // Apply the security scheme globally to all endpoints
                //c.AddSecurityRequirement(document => new
                //{
                //    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                //});
            });

            // __ Configure CORS Policies __ //
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                options.AddPolicy("ZU-DCMS-React", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:8080")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // __ Configure Rate Limiting (Basic fixed window limiter) __ //
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100, // Max 100 requests per window
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1) // 1-minute window
                        }));
            });

            return services;
        }
    }
}
