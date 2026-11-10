using Microsoft.EntityFrameworkCore;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Artist> Artists { get; }
    DbSet<Track> Tracks { get; }
    DbSet<Dsp> Dsps { get; }
    DbSet<TrackDistribution> TrackDistributions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
