using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Contracts;
using ZU_DCMS.Domain.Interfaces;
using ZU_DCMS.INFRASTRUCTURE.Cache;
using ZU_DCMS.INFRASTRUCTURE.Identity;
using ZU_DCMS.INFRASTRUCTURE.Persistence;
using ZU_DCMS.INFRASTRUCTURE.Persistence.Repositories;

namespace ZU_DCMS.INFRASTRUCTURE.Extentions
{
    public static class InfrastructureServiceExtensions
    {
        // Extension method to add infrastructure services to the DI container
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database 
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),

            // Specify the assembly for migrations
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings set to be simple
                // Gonna be validated in fluent validation instead for better user experience
                // As this ux will use the national number as the password, which is more than 10 digits long
                // And doesn't contain uppercase, lowercase, or non-alphanumeric characters
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // Lockout settings if the user fails to login after 5 attempts, lock them out for 15 minutes
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                // User settings - require unique email addresses
                options.User.RequireUniqueEmail = true;
            })
            // Use our AppDbContext for Identity
            .AddEntityFrameworkStores<AppDbContext>()

            // Add default token providers for password reset, email confirmation, etc.
            .AddDefaultTokenProviders();

            // Add repositories and unit of work to the DI container
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add caching services
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();

            return services;
        }
    }
}
