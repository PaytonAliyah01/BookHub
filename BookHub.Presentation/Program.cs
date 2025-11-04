using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Add Razor Pages
builder.Services.AddRazorPages();

// 2️⃣ Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";        // redirect to login page if not authenticated
        options.AccessDeniedPath = "/Login"; // redirect if unauthorized
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // cookie lifetime
    });

// 3️⃣ Optional: session support (if you plan to use HttpContext.Session)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 4️⃣ Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();           // optional, before authentication
app.UseAuthentication();    // must come before UseAuthorization
app.UseAuthorization();

app.MapRazorPages();

app.Run();
