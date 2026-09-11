using Microsoft.EntityFrameworkCore;
using VitalSync.DoctorService.Domain.Entities;
using VitalSync.DoctorService.Domain.Repositories;
using VitalSync.DoctorService.Infrastructure.Persistence;
using VitalSync.DoctorService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=VitalSync_DoctorDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<DoctorDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();

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
    var db = scope.ServiceProvider.GetRequiredService<DoctorDbContext>();
    db.Database.EnsureCreated();

    if (!db.Doctors.Any())
    {
        db.Doctors.AddRange(
            new Doctor("Dr. Robert Chen, MD", "Cardiology", "DOC-CARD-001"),
            new Doctor("Dr. Sarah Jenkins, MD", "Pulmonology", "DOC-PULM-002"),
            new Doctor("Dr. Alex Vance, MD", "EmergencyMedicine", "DOC-EMER-003")
        );
        db.SaveChanges();
    }
}

app.Run();
