using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(b => b.AddService(builder.Environment.ApplicationName))
    .WithTracing(b => b.AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation())
    .WithMetrics(b => b.AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(opt =>
{
    opt.IncludeScopes = true;
    opt.IncludeFormattedMessage = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapControllers();

await app.RunAsync();
