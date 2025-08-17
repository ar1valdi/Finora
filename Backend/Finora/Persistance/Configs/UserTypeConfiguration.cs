using Finora.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finora.Persistance.Configs
{
    internal class UserTypeConfiguration : EntityTypeConfigurationBase<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.Property(u => u.DateOfBirth)
                   .HasColumnType("timestamp without time zone");
        }
    }
}
