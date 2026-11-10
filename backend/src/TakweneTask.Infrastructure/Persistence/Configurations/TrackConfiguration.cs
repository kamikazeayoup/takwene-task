using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Infrastructure.Persistence.Configurations;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Isrc)
            .IsRequired()
            .HasMaxLength(12);

        builder.HasIndex(t => t.Isrc)
            .IsUnique();

        builder.Property(t => t.ReleaseDate)
            .IsRequired();

        builder.Property(t => t.Genre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(t => t.Artist)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(t => t.Distributions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
