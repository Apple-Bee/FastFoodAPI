using Microsoft.AspNetCore.Identity;

namespace FastFoodAPI.Data
{
    public class IdentitySeed
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Create Admin role if it doesn't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create an admin user if it doesn't exist
            var adminEmail = "admin@fastfood.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                await userManager.CreateAsync(admin, "AdminPassword123!"); // Use a strong password
                await userManager.AddToRoleAsync(admin, "Admin"); // Assign Admin role
            }
        }
    }
}
