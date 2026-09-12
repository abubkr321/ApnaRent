using ApnaRent.Data;
using ApnaRent.Models;
using Microsoft.AspNetCore.Identity;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        // Default Admin
        var adminEmail = "admin@apnarent.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin"
            };

            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

    }
    public static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Electronics" },
                new Category { Name = "Vehicles" },
                new Category { Name = "Furniture" },
                new Category { Name = "Event & Party" },
                new Category { Name = "Sports & Outdoors" },
                new Category { Name = "Picnic & Camping" },
                new Category { Name = "Kitchen Appliances" },
                new Category { Name = "Tools & Equipment" }
            );
            await context.SaveChangesAsync();
        }
    }
}
