using Finora.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finora.Backend.Persistance.Configs.Database
{
    internal class BankTransactionTypeConfiguration : EntityTypeConfigurationBase<BankTransaction>
    {
        public override void Configure(EntityTypeBuilder<BankTransaction> builder)
        {
            base.Configure(builder);

            builder.Property(t => t.Amount)
                .IsRequired();

            builder.Property(t => t.TransactionDate)
                .IsRequired();

            builder.Property(t => t.Description)
                .IsRequired();

            builder.HasOne(t => t.From)
                .WithMany(a => a.OutgoingTransactions)
                .HasForeignKey(t => t.FromId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.To)
                .WithMany(a => a.IncomingTransactions)
                .HasForeignKey(t => t.ToId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


