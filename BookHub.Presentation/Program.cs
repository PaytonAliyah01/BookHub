using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.DAL.Interfaces;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddScoped<IBookDAL>(provider => new BookDAL(connectionString));
builder.Services.AddScoped<IUserDAL>(provider => new UserDAL(connectionString));
builder.Services.AddScoped<IUserBookshelfDAL>(provider => new UserBookshelfDAL(connectionString));
builder.Services.AddScoped<IAdminDAL>(provider => new AdminDAL(connectionString));
builder.Services.AddScoped<IBookClubDAL>(provider => new BookClubDAL_Simple(connectionString));
builder.Services.AddScoped<IBookReviewDAL>(provider => new BookReviewDAL(connectionString));
builder.Services.AddScoped<IBookBLL, BookBLL>();
builder.Services.AddScoped<IReadingGoalBLL>(provider => new ReadingGoalBLL(connectionString));
builder.Services.AddScoped<IBookReviewBLL>(provider => new BookReviewBLL(connectionString));
builder.Services.AddScoped<IUserBLL>(provider => new UserBLL(connectionString));
builder.Services.AddScoped<IUserBookshelfBLL>(provider => new UserBookshelfBLL(connectionString));
builder.Services.AddScoped<IBookClubBLL>(provider => new BookClubBLL(connectionString));
builder.Services.AddScoped<IForumBLL>(provider => new ForumBLL(connectionString));
builder.Services.AddScoped<IFriendBLL>(provider => new FriendBLL(connectionString));
builder.Services.AddScoped<IAnalyticsBLL, AnalyticsBLL>();
builder.Services.AddScoped<AdminBLL>();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.Run();
