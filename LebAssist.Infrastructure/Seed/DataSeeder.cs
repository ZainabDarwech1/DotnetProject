using Domain.Entities;
using LebAssist.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LebAssist.Infrastructure.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager, context);

            // Seed Categories
            await SeedCategoriesAsync(context);

            // Seed Services
            await SeedServicesAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Client", "Provider" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            string adminEmail = "admin@lebassist.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Create Client profile for admin
                    var adminClient = new Client
                    {
                        AspNetUserId = adminUser.Id,
                        FirstName = "System",
                        LastName = "Admin",
                        PhoneNumber = "00000000",
                        Latitude = 33.8938m,
                        Longitude = 35.5018m,
                        IsProvider = false,
                        DateRegistered = DateTime.UtcNow
                    };

                    context.Clients.Add(adminClient);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.ServiceCategories.AnyAsync())
                return;

            var categories = new List<ServiceCategory>
            {
                new ServiceCategory
                {
                    CategoryName = "Home Repair",
                    Description = "General home repair and maintenance services",
                    IsActive = true,
                    DisplayOrder = 1
                },
                new ServiceCategory
                {
                    CategoryName = "Cleaning",
                    Description = "Home and office cleaning services",
                    IsActive = true,
                    DisplayOrder = 2
                },
                new ServiceCategory
                {
                    CategoryName = "Electrical",
                    Description = "Electrical installation and repair services",
                    IsActive = true,
                    DisplayOrder = 3
                },
                new ServiceCategory
                {
                    CategoryName = "Plumbing",
                    Description = "Plumbing installation and repair services",
                    IsActive = true,
                    DisplayOrder = 4
                },
                new ServiceCategory
                {
                    CategoryName = "Moving",
                    Description = "Moving and transportation services",
                    IsActive = true,
                    DisplayOrder = 5
                },
                new ServiceCategory
                {
                    CategoryName = "Painting",
                    Description = "Interior and exterior painting services",
                    IsActive = true,
                    DisplayOrder = 6
                },
                new ServiceCategory
                {
                    CategoryName = "Appliance Repair",
                    Description = "Home appliance repair services",
                    IsActive = true,
                    DisplayOrder = 7
                },
                new ServiceCategory
                {
                    CategoryName = "Gardening",
                    Description = "Garden maintenance and landscaping services",
                    IsActive = true,
                    DisplayOrder = 8
                }
            };

            await context.ServiceCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedServicesAsync(ApplicationDbContext context)
        {
            if (await context.Services.AnyAsync())
                return;

            var categories = await context.ServiceCategories.ToListAsync();

            var services = new List<Service>();

            // Home Repair Services
            var homeRepair = categories.FirstOrDefault(c => c.CategoryName == "Home Repair");
            if (homeRepair != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = homeRepair.CategoryId, ServiceName = "Door Repair", ServiceDescription = "Repair and installation of doors", IsActive = true },
                    new Service { CategoryId = homeRepair.CategoryId, ServiceName = "Window Repair", ServiceDescription = "Repair and installation of windows", IsActive = true },
                    new Service { CategoryId = homeRepair.CategoryId, ServiceName = "Furniture Assembly", ServiceDescription = "Assembly of furniture items", IsActive = true }
                });
            }

            // Cleaning Services
            var cleaning = categories.FirstOrDefault(c => c.CategoryName == "Cleaning");
            if (cleaning != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = cleaning.CategoryId, ServiceName = "House Cleaning", ServiceDescription = "Complete house cleaning service", IsActive = true },
                    new Service { CategoryId = cleaning.CategoryId, ServiceName = "Office Cleaning", ServiceDescription = "Office and commercial cleaning", IsActive = true },
                    new Service { CategoryId = cleaning.CategoryId, ServiceName = "Deep Cleaning", ServiceDescription = "Thorough deep cleaning service", IsActive = true }
                });
            }

            // Electrical Services
            var electrical = categories.FirstOrDefault(c => c.CategoryName == "Electrical");
            if (electrical != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = electrical.CategoryId, ServiceName = "Wiring Installation", ServiceDescription = "Electrical wiring installation", IsActive = true },
                    new Service { CategoryId = electrical.CategoryId, ServiceName = "Light Fixture Installation", ServiceDescription = "Installation of light fixtures", IsActive = true },
                    new Service { CategoryId = electrical.CategoryId, ServiceName = "Electrical Repair", ServiceDescription = "General electrical repairs", IsActive = true }
                });
            }

            // Plumbing Services
            var plumbing = categories.FirstOrDefault(c => c.CategoryName == "Plumbing");
            if (plumbing != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = plumbing.CategoryId, ServiceName = "Pipe Repair", ServiceDescription = "Repair of water pipes", IsActive = true },
                    new Service { CategoryId = plumbing.CategoryId, ServiceName = "Drain Cleaning", ServiceDescription = "Cleaning of blocked drains", IsActive = true },
                    new Service { CategoryId = plumbing.CategoryId, ServiceName = "Faucet Installation", ServiceDescription = "Installation and repair of faucets", IsActive = true }
                });
            }

            // Moving Services
            var moving = categories.FirstOrDefault(c => c.CategoryName == "Moving");
            if (moving != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = moving.CategoryId, ServiceName = "Local Moving", ServiceDescription = "Moving within the city", IsActive = true },
                    new Service { CategoryId = moving.CategoryId, ServiceName = "Packing Service", ServiceDescription = "Professional packing service", IsActive = true }
                });
            }

            // Painting Services
            var painting = categories.FirstOrDefault(c => c.CategoryName == "Painting");
            if (painting != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = painting.CategoryId, ServiceName = "Interior Painting", ServiceDescription = "Interior wall painting", IsActive = true },
                    new Service { CategoryId = painting.CategoryId, ServiceName = "Exterior Painting", ServiceDescription = "Exterior wall painting", IsActive = true }
                });
            }

            // Appliance Repair Services
            var appliance = categories.FirstOrDefault(c => c.CategoryName == "Appliance Repair");
            if (appliance != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = appliance.CategoryId, ServiceName = "AC Repair", ServiceDescription = "Air conditioner repair and maintenance", IsActive = true },
                    new Service { CategoryId = appliance.CategoryId, ServiceName = "Washing Machine Repair", ServiceDescription = "Washing machine repair", IsActive = true },
                    new Service { CategoryId = appliance.CategoryId, ServiceName = "Refrigerator Repair", ServiceDescription = "Refrigerator repair", IsActive = true }
                });
            }

            // Gardening Services
            var gardening = categories.FirstOrDefault(c => c.CategoryName == "Gardening");
            if (gardening != null)
            {
                services.AddRange(new[]
                {
                    new Service { CategoryId = gardening.CategoryId, ServiceName = "Lawn Mowing", ServiceDescription = "Lawn mowing and maintenance", IsActive = true },
                    new Service { CategoryId = gardening.CategoryId, ServiceName = "Tree Trimming", ServiceDescription = "Tree and hedge trimming", IsActive = true },
                    new Service { CategoryId = gardening.CategoryId, ServiceName = "Garden Design", ServiceDescription = "Garden design and landscaping", IsActive = true }
                });
            }

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }
    }
}