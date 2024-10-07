using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using TimeOffManager.DataAccess.Models;

namespace TimeOffManager.DataAccess.Configurations
{
    public class ManagerDbConfig : IEntityTypeConfiguration<Manager>
    {
        public void Configure(EntityTypeBuilder<Manager> builder)
        {
            builder.HasMany(obj => obj.PersonalTimeOffs).WithOne(x => x.Manager).HasForeignKey(f => f.ManagerId);
            builder.HasMany(obj => obj.TimeOffsApprove).WithOne(x => x.ApproveManager).HasForeignKey(f => f.ManagerId);
        }
    }
}
