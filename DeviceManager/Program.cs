using System;
using Microsoft.EntityFrameworkCore;
using DeviceManager.Data;
using SQLitePCL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Enable global authorization
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

Batteries.Init();

// App DB
builder.Services.AddDbContext<DeviceContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DeviceManager")));



// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<DeviceContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    // Use the custom LoginController instead of Identity UI login page
    options.LoginPath = "/Account";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    options.Cookie.Name = "DeviceManager.Identity.Cookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    options.ReturnUrlParameter = "returnUrl";
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
