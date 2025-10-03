using BookHub.Data;
using BookHub.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Register DbContext with SQL Server
builder.Services.AddDbContext<BookHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register UserService for dependency injection
builder.Services.AddScoped<UserService>();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Build the app
var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // Serve static files like CSS/JS/images

app.UseRouting();

app.UseSession();       // Make sure session is before authorization
app.UseAuthorization();

app.MapRazorPages();

app.Run();
