using BlipApp.Data;
using BlipApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlipApp.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService
                <DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any())
                {
                    return;  
                }

                context.Roles.AddRange(
                    new IdentityRole { Id = "16d5ff73-8270-4a29-ac47-7cecba40a6c0", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                    new IdentityRole { Id = "16d5ff73-8270-4a29-ac47-7cecba40a6c1", Name = "Registered", NormalizedName = "Registered".ToUpper() },
                    new IdentityRole { Id = "16d5ff73-8270-4a29-ac47-7cecba40a6c2", Name = "Unregistered", NormalizedName = "Unregistered".ToUpper() }
                );

                var hasher = new PasswordHasher<ApplicationUser>();

                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "1a35622e-0f98-42f1-b314-844d524f5ab0", // primary key
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    },
                    new ApplicationUser
                    {
                        Id = "1a35622e-0f98-42f1-b314-844d524f5ab1", // primary key
                        UserName = "registered@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "REGISTERED@TEST.COM",
                        Email = "REGISTERED@test.com",
                        NormalizedUserName = "REGISTERED@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Registered1!")
                    },
                    new ApplicationUser
                    {
                        Id = "1a35622e-0f98-42f1-b314-844d524f5ab2", // primary key
                        UserName = "Unregistered@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "UNREGISTERED@TEST.COM",
                        Email = "unregistered@test.com",
                        NormalizedUserName = "UNREGISTERED@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Unregistered1!")
                    }
                );

                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "16d5ff73-8270-4a29-ac47-7cecba40a6c0",
                        UserId = "1a35622e-0f98-42f1-b314-844d524f5ab0"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "16d5ff73-8270-4a29-ac47-7cecba40a6c1",
                        UserId = "1a35622e-0f98-42f1-b314-844d524f5ab1"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "16d5ff73-8270-4a29-ac47-7cecba40a6c2",
                        UserId = "1a35622e-0f98-42f1-b314-844d524f5ab2"
                    }
                );

                context.SaveChanges();

            }
        }
    }
}
