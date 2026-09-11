using Microsoft.EntityFrameworkCore;
using VitalSync.LabService.Domain.Entities;
using VitalSync.LabService.Domain.Repositories;
using VitalSync.LabService.Infrastructure.Persistence;
using VitalSync.LabService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=VitalSync_LabDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<LabDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ILabRepository, LabRepository>();

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
    var db = scope.ServiceProvider.GetRequiredService<LabDbContext>();
    db.Database.EnsureCreated();

    if (!db.Equipment.Any())
    {
        db.Equipment.AddRange(
            new LabEquipment("Digital ECG Monitor 01", "ECG", "Bay A-1"),
            new LabEquipment("Cardiac Troponin Analyzer", "Troponin", "Bay A-2"),
            new LabEquipment("High-Resolution Digital X-Ray", "XRay", "Bay B-1"),
            new LabEquipment("64-Slice CT Scanner", "CTScan", "Bay C-1")
        );
        db.SaveChanges();
    }
}

app.Run();
