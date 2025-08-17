using Finora.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public abstract class EntityTypeConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase
{
    protected virtual string Schema => "Finora";
    protected virtual string TableName => typeof(TEntity).Name;

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
               .IsRequired();
    }
}