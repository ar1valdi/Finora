using Finora.Kernel;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finora.Backend.Persistance.Configs.Database
{
    internal class OutboxMessageTypeConfiguration : EntityTypeConfigurationBase<OutboxMessage>
    {
        public override void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            base.Configure(builder);
        }
    }
}
