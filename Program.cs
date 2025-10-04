using BookHub.Data;
using BookHub.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddDbContext<BookHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UserService>();
builder.Services.AddHttpContextAccessor();

// Add authentication and cookie scheme
builder.Services.AddAuthentication("BookHubCookieAuth")
    .AddCookie("BookHubCookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";   // Redirect here if not logged in
        options.LogoutPath = "/Auth/Logout"; // Redirect here on logout
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// Add session if needed (optional now)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication middleware before authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();
