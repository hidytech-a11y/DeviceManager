using DeviceManager.Data;
using DeviceManager.Services;
using DeviceManager.Services.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<DeviceContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Get connection string from either config or environment variable
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

// Convert PostgreSQL URL format to Npgsql format if needed
if (!string.IsNullOrEmpty(connectionString))
{
    if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
    {
        var uri = new Uri(connectionString);
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.LocalPath.TrimStart('/').Split('?')[0];
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";

        connectionString = $"Host=ep-bitter-truth-ael6vn5q-pooler.c-2.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_a68YyAxkZnjb;SSL Mode=Require;Trust Server Certificate=true;";
    }
}

builder.Services.AddDbContext<DeviceContext>(options =>
    options.UseNpgsql(connectionString));


//Services for Dependency Injection
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDeviceHistoryService, DeviceHistoryService>();
builder.Services.AddScoped<IAdminOverrideService, AdminOverrideService>();


// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<DeviceContext>()
.AddDefaultTokenProviders();


// MVC
builder.Services.AddControllersWithViews(options =>
{
    // Redirect unauthorized users
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TechnicianOrAdminOverride", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Technician") ||
            (context.User.IsInRole("Admin") &&
             builder.Configuration.GetValue<bool>("AdminOverride:Enabled"))
        ));

    options.AddPolicy("ManagerOrAdminOverride", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Manager") ||
            (context.User.IsInRole("Admin") &&
             builder.Configuration.GetValue<bool>("AdminOverride:Enabled"))
        ));
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeviceContext>();
    db.Database.Migrate();
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// RBAC seed
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = ["Admin", "Manager", "Technician", "Viewer"];

    foreach (var role in roles)
    {
        var exists = await roleManager.RoleExistsAsync(role);
        if (!exists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Default admin
    string adminEmail = "admin@local.dev";
    string adminPassword = "Admin1234";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var createUser = await userManager.CreateAsync(newAdmin, adminPassword);

        if (createUser.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}


// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "accessDenied",
    pattern: "Account/AccessDenied",
    defaults: new { controller = "Account", action = "AccessDenied" });

app.Run();
