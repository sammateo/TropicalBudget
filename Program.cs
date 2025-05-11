using System.Globalization;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Debug = builder.Environment.IsDevelopment(); // Enable debug mode for development
    options.SendDefaultPii = true; // Adds request URL and headers, IP and name for users, etc.
    options.TracesSampleRate = 1.0; // Set this to configure automatic tracing
    options.Environment = builder.Environment.IsDevelopment() ? "development" : "production";
    options.AutoSessionTracking = true; // This option is recommended. It enables Sentry's "Release Health" feature.
    options.IsGlobalModeEnabled = false; // Enabling this option is recommended for client applications only. It ensures all threads use the same global scope.
    options.SetBeforeSend((@event, hint) =>
    {
        // Never report server names
        @event.ServerName = null;
        return @event;
    });
    //options.Release = builder.Configuration["Sentry:Release"];
}); // Initialize Sentry
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

