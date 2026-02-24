using DeviceManager.Data;
using DeviceManager.Services;
using DeviceManager.Services.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDeviceHistoryService, DeviceHistoryService>();




//builder.Services.AddDbContext<DeviceContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');
    var username = userInfo[0];
    var password = userInfo[1];

    connectionString =
        $"Host=dpg-d6eoga3uibrs73didn30-a.oregon-postgres.render.com;Port=5432;Database=devicemanager_h33j;Username=devicemanager_h33j_user;Password=EplH2cTcZKjb8BUawAmwkehpcON46N3h;SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<DeviceContext>(options =>
    options.UseNpgsql(connectionString));

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

builder.Services.AddScoped<IAdminOverrideService, AdminOverrideService>();

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
