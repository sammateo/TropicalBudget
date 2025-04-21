using Auth0.AspNetCore.Authentication;
using TropicalBudget.Services;
using TropicalBudget.Utilities;

var builder = WebApplication.CreateBuilder(args);
// Cookie configuration for HTTP to support cookies with SameSite=None
//builder.Services.Configure<CookiePolicyOptions>(options =>
//{
//    options.Secure = CookieSecurePolicy.Always;
//    options.HttpOnly = HttpOnlyPolicy.Always;
//    options.MinimumSameSitePolicy = SameSiteMode.None;
//});
builder.Services.ConfigureSameSiteNoneCookies();
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DatabaseService>();

var app = builder.Build();

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

