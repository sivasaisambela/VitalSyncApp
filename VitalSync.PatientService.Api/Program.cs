
using Microsoft.EntityFrameworkCore;
using VitalSync.PatientService.Api.Middleware;
using VitalSync.PatientService.Application.Services;
using VitalSync.PatientService.Domain.Repositories;
using VitalSync.PatientService.Infrastructure.Persistence;
using VitalSync.PatientService.Infrastructure.Repositories;

namespace VitalSync.PatientService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 2. Retrieve Connection String with Fail-Fast Validation (Enterprise Standard)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in application configuration.");


            // 3. Register EF Core DbContext
            builder.Services.AddDbContext<PatientDbContext>(options =>
                options.UseSqlServer(connectionString, b =>
                    b.MigrationsAssembly("VitalSync.PatientService.Infrastructure")));

            // 4. Register Repositories (Infrastructure -> Domain)
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<ITriageRecordRepository, TriageRecordRepository>();
            // 5. Register Application Services (Application -> API)
            builder.Services.AddScoped<IPatientAppService, PatientAppService>();
            builder.Services.AddScoped<ITriageAppService, TriageAppService>();
            // 6. Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();


            // 1. REGISTER GLOBAL EXCEPTION MIDDLEWARE FIRST
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            // Ensure Database schema exists on startup in Development
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
                dbContext.Database.EnsureCreated();
            }
            app.Run();
        }
    }
}
