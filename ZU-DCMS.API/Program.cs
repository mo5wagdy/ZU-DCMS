using ZU_DCMS.INFRASTRUCTURE.Extentions;
using ZU_DCMS.APPLICATION.Extentions;
using Hangfire;

namespace ZU_DCMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Add infrastructure services
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Add application services
            builder.Services.AddApplicationServices();

            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire");

            app.MapControllers();

            app.Run();
        }
    }
}
