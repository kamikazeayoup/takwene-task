using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Infrastructure.Persistence.Configurations;

public class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.ToTable("track_distributions");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.SubmittedAt)
            .IsRequired();

        builder.HasIndex(d => new { d.TrackId, d.DspId })
            .IsUnique();

        builder.HasOne(d => d.Track)
            .WithMany(t => t.Distributions)
            .HasForeignKey(d => d.TrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Dsp)
            .WithMany()
            .HasForeignKey(d => d.DspId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
