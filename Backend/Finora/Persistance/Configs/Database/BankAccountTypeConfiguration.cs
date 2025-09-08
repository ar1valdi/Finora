using Finora.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finora.Backend.Persistance.Configs.Database
{
    internal class BankAccountTypeConfiguration : EntityTypeConfigurationBase<BankAccount>
    {
        public override void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            base.Configure(builder);

            builder.Property(b => b.IsClosed)
                .IsRequired();

            builder.Property(b => b.Balance)
                .IsRequired();

            builder.HasOne(b => b.User)
                .WithOne(u => u.BankAccount)
                .HasForeignKey<BankAccount>("UserId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


