using Microsoft.Extensions.DependencyInjection;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.DataAccess.Extensions
{
    public static class RolesDbHelper
    {
        public static void AddDefaultDbRoles(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var roleManager = provider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<ManagerRole>>();

            var roles = new string[] { DbConstants.OwnerRoleName, DbConstants.ManagerRoleName };

            foreach (var role in roles)
            {
                var roleExists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();

                if (!roleExists)
                {
                    roleManager.CreateAsync(new ManagerRole()
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    }).GetAwaiter().GetResult();
                }
            }

        }
    }
}
