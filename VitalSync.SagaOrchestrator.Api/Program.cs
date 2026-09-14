using Microsoft.EntityFrameworkCore;
using VitalSync.SagaOrchestrator.Application.HttpClients;
using VitalSync.SagaOrchestrator.Application.Services;
using VitalSync.SagaOrchestrator.Domain.Repositories;
using VitalSync.SagaOrchestrator.Infrastructure.HttpClients;
using VitalSync.SagaOrchestrator.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Retrieve Connection String with Fail-Fast Validation (Enterprise Standard)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in application configuration.");


// 3. Register EF Core DbContext


builder.Services.AddDbContext<SagaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ISagaRepository, SagaRepository>();
builder.Services.AddScoped<IEmergencyReservationSagaManager, EmergencyReservationSagaManager>();

string doctorServiceUrl = builder.Configuration["Services:DoctorService"] ?? "https://localhost:7050";
string labServiceUrl = builder.Configuration["Services:LabService"] ?? "https://localhost:7051";
string apiKey = builder.Configuration["Security:InternalApiKey"] ?? "VitalSyncInternalSecretKey2026!";

builder.Services.AddHttpClient<IDoctorServiceClient, DoctorServiceClient>(client =>
{
    client.BaseAddress = new Uri(doctorServiceUrl);
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
});

builder.Services.AddHttpClient<ILabServiceClient, LabServiceClient>(client =>
{
    client.BaseAddress = new Uri(labServiceUrl);
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
