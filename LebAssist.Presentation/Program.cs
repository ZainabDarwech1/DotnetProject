using LebAssist.Application;
using LebAssist.Application.Common;
using LebAssist.Infrastructure;
using LebAssist.Infrastructure.Auth;
using LebAssist.Infrastructure.Data;
using LebAssist.Infrastructure.Seed;
using LebAssist.Presentation.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// R11.6: Configure max request body size (50MB for file uploads)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

// Register Services
builder.Services.AddSignalR();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Google Maps Settings (R10)
builder.Services.Configure<GoogleMapsSettings>(builder.Configuration.GetSection("GoogleMaps"));

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await context.Database.MigrateAsync();
        await DataSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// R11.6: Create upload directories
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(Path.Combine(uploadsPath, "profiles"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "portfolio"));
Directory.CreateDirectory(Path.Combine(uploadsPath, "emergencies"));

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

// Area routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// SignalR Hubs (R9)
app.MapHub<EmergencyHub>("/hubs/emergency");
app.MapHub<AvailabilityHub>("/hubs/availability");
app.MapHub<BookingHub>("/hubs/booking");
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();