using Finora.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finora.Backend.Persistance.Configs.Database
{
    internal class UserTypeConfiguration : EntityTypeConfigurationBase<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
        
            builder.HasOne(u => u.BankAccount)
                .WithOne(b => b.User)
                .HasForeignKey<User>("BankAccountId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
