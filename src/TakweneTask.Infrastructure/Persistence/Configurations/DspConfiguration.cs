using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Infrastructure.Persistence.Configurations;

public class DspConfiguration : IEntityTypeConfiguration<Dsp>
{
    public void Configure(EntityTypeBuilder<Dsp> builder)
    {
        builder.ToTable("dsps");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => d.Name)
            .IsUnique();
    }
}
