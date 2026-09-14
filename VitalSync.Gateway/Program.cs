using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// 1. Add YARP Reverse Proxy & Load Configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        // Tech Lead Security Feature: Inject Internal Secret API Key header to all outgoing microservice requests
        builderContext.AddRequestHeader("X-Internal-Api-Key", "VitalSync_Secure_Internal_Secret_2026!");
    });

// 2. Configure CORS for Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Critical for WebSockets / SignalR
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// 3. Map Reverse Proxy Endpoints
app.MapReverseProxy();

app.Run();
