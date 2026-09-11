
using VitalSync.VitalsService.Hubs;
using VitalSync.VitalsService.Services;

namespace VitalSync.VitalsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // 1. Add SignalR Services
            builder.Services.AddSignalR();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 2. Register Background Vitals Simulator
            builder.Services.AddHostedService<VitalsBackgroundSimulator>();
            // 3. Configure CORS (Required for WebSockets from Frontend)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Critical for WebSockets/SignalR!
                });
            });
            var app = builder.Build();

            app.UseCors("AllowAll");
            app.UseRouting();
            // 4. Map SignalR Hub Endpoint
            app.MapHub<VitalsHub>("/hubs/vitals");
            app.MapControllers();
            app.Run();
        }
    }
}
