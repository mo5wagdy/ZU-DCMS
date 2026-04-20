using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZU_DCMS.APPLICATION.Common.Auth;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.APPLICATION.Contracts.Auth;
using ZU_DCMS.APPLICATION.Contracts.Cache;
using ZU_DCMS.APPLICATION.Contracts.Logger;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.INFRASTRUCTURE.Cache;
using ZU_DCMS.INFRASTRUCTURE.Identity;
using ZU_DCMS.INFRASTRUCTURE.Identity.ContractImplementation;
using ZU_DCMS.INFRASTRUCTURE.Identity.Unique_Validations;
using ZU_DCMS.INFRASTRUCTURE.Persistence;
using ZU_DCMS.INFRASTRUCTURE.Persistence.ContractImplementation;
using ZU_DCMS.INFRASTRUCTURE.Persistence.InterfacesImplementations;

namespace ZU_DCMS.INFRASTRUCTURE
{
    public static class DependencyInjection
    {
        // _____________________ Extension method to add infrastructure services to the DI container _____________________ //
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // _____________________ Database _____________________ // 
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),

            // _________________ Specify the assembly for migrations _________________ //
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // __________ Hangfire __________ //
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            });
            // __________ Hangfire Server __________ //
            services.AddHangfireServer();

            // _____________________ Identity _____________________ //
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                /* 
                 * Password settings set to be simple
                 * Gonna be validated in fluent validation instead for better user experience
                 * As this ux will use the national number as the password, which is more than 10 digits long
                 * And doesn't contain uppercase, lowercase, or non-alphanumeric characters
                 */
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // __ Lockout settings if the user fails to login after 5 attempts, lock them out for 15 minutes __ //
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                // __ User settings - require unique email addresses __ //
                options.User.RequireUniqueEmail = true;
            })
            // __ Use our AppDbContext for Identity __ //
            .AddEntityFrameworkStores<AppDbContext>()

            // __ Unique Password validator for staff accounts to ensure they don't use the national number as their password __ //
            .AddPasswordValidator<UniquePasswordValidator<ApplicationUser>>()

            // __ Add default token providers for password reset, email confirmation, etc. __ //
            .AddDefaultTokenProviders();

            /// _________________ Add raw SQL executor service to the DI container ________________ 
            /// This service will be used to execute raw SQL queries and commands against the database,
            /// which is useful for complex queries or performance-critical operations that cannot be easily achieved with Entity Framework Core's LINQ capabilities.///
            services.AddScoped<IRawSqlExecutor, RawSqlExecutor>();

            /// __ Add user code generator service to the DI container __ //
            /// This service will be responsible for generating unique user codes, which can be used for various purposes such as user identification, referral codes, etc.
            /// The implementation of this service will ensure that the generated codes are unique and follow a specific format if needed.///
            services.AddScoped<IUserCodeGenerator, UserCodeGenerator>();

            // _____________________ JWT Settings _____________________ //
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // __ Add JWT service to the DI container __ //
            services.AddScoped<IJWTService, JwtService>();

            // __ Add identity service to the DI container __ //
            services.AddScoped<IIdentityService, IdentityService>();

            // __ Add application logger to the DI container __ //
            services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

            // __ Add repositories and unit of work to the DI container __ //
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // _______________ Add caching services _______________ //
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();

            return services;
        }
    }
}