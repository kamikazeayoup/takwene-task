using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Infrastructure.Persistence.Configurations;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("artists");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.HasIndex(a => a.Email)
            .IsUnique();

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(2);
    }
}
