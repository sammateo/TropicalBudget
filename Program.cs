using System.Globalization;
using Auth0.AspNetCore.Authentication;
using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(configure =>
    {
        configure.UseGrafana()
            .AddConsoleExporter();
    })
    .WithMetrics(configure =>
    {
        configure.UseGrafana()
            .AddConsoleExporter();
    });
builder.Logging.AddOpenTelemetry(options =>
{
    options.UseGrafana()
        .AddConsoleExporter();
});
Environment.SetEnvironmentVariable("OTEL_RESOURCE_ATTRIBUTES", builder.Configuration["Grafana:OTEL_RESOURCE_ATTRIBUTES"]);
Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", builder.Configuration["Grafana:OTEL_EXPORTER_OTLP_ENDPOINT"]);
Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS", builder.Configuration["Grafana:OTEL_EXPORTER_OTLP_HEADERS"]);
Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL", builder.Configuration["Grafana:OTEL_EXPORTER_OTLP_PROTOCOL"]);
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .UseGrafana()
            .Build();
using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .UseGrafana()
    .Build();

//https://community.auth0.com/t/redirect-uri-is-always-http-but-only-in-production/83978/4
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
});
builder.Services.ConfigureSameSiteNoneCookies();
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    options.Scope = "openid profile email";
});

CultureInfo culture = new CultureInfo("en-US");
culture.NumberFormat.CurrencySymbol = "$";
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DatabaseService>();

var app = builder.Build();
app.UseForwardedHeaders();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

