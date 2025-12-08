using DeviceManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<DeviceContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DeviceManager")));

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

var app = builder.Build();

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

    string[] roles = { "Admin", "Manager", "Technician", "Viewer" };

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
