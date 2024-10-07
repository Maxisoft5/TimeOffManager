using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.DataAccess.Context
{
    public class DataContext : IdentityUserContext<User, int>
    {
        public DbSet<ManagerRole> ManagerRoles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<TimeOff> TimeOffs { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
         : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUserRole<int>>().HasKey(p => new { p.UserId, p.RoleId });
            modelBuilder.Entity<IdentityRoleClaim<int>>();
        }
    }
}
