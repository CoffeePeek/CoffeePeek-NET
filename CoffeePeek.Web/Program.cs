using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Настройка для прослушивания на IPv6 (требуется для Railway приватной сети)
// IPv6 слушает на всех интерфейсах, включая IPv4 через IPv6-mapped адреса
builder.WebHost.UseUrls("http://[::]:80");

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
// Enable cookie-based authentication
    .AddCookie()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration.GetSection("OAuthGoogleOptions:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("OAuthGoogleOptions:ClientSecret").Value;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseMiddleware<CoffeePeek.Web.Middleware.AuthTokenMiddleware>();

app.UseAuthorization();

app.MapRazorPages();

Console.WriteLine("Listening on: http://[::]:80 (IPv6 for Railway private network)");

app.Run();