using Microsoft.EntityFrameworkCore;
using TakweneTask.Domain.Entities;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Infrastructure.Persistence.Seed;

public static class SeedData
{
    // Fixed UUIDv7 IDs for DSPs
    public static readonly Guid SpotifyId = new("019318a0-0001-7000-8000-000000000001");
    public static readonly Guid AppleMusicId = new("019318a0-0002-7000-8000-000000000002");
    public static readonly Guid YouTubeId = new("019318a0-0003-7000-8000-000000000003");

    // Fixed UUIDv7 IDs for Artists
    public static readonly Guid ArtistNourId = new("019318a0-0011-7000-8000-000000000001");
    public static readonly Guid ArtistLaylaId = new("019318a0-0012-7000-8000-000000000002");
    public static readonly Guid ArtistOmarId = new("019318a0-0013-7000-8000-000000000003");
    public static readonly Guid ArtistYasmineId = new("019318a0-0014-7000-8000-000000000004");

    // Fixed UUIDv7 IDs for Tracks
    public static readonly Guid Track1Id = new("019318a0-0021-7000-8000-000000000001");
    public static readonly Guid Track2Id = new("019318a0-0022-7000-8000-000000000002");
    public static readonly Guid Track3Id = new("019318a0-0023-7000-8000-000000000003");
    public static readonly Guid Track4Id = new("019318a0-0024-7000-8000-000000000004");
    public static readonly Guid Track5Id = new("019318a0-0025-7000-8000-000000000005");
    public static readonly Guid Track6Id = new("019318a0-0026-7000-8000-000000000006");
    public static readonly Guid Track7Id = new("019318a0-0027-7000-8000-000000000007");
    public static readonly Guid Track8Id = new("019318a0-0028-7000-8000-000000000008");
    public static readonly Guid Track9Id = new("019318a0-0029-7000-8000-000000000009");
    public static readonly Guid Track10Id = new("019318a0-002a-7000-8000-000000000010");

    // Fixed UTC Dates
    private static readonly DateTime BaseSeedTime = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        // 1. Seed DSPs
        modelBuilder.Entity<Dsp>().HasData(
            new { Id = SpotifyId, Name = "Spotify" },
            new { Id = AppleMusicId, Name = "Apple Music" },
            new { Id = YouTubeId, Name = "YouTube" }
        );

        // 2. Seed Artists
        modelBuilder.Entity<Artist>().HasData(
            new { Id = ArtistNourId, Name = "Nour El Sabah", Email = "nour.elsabah@takwene.com", Country = "EG" },
            new { Id = ArtistLaylaId, Name = "Layla Haddad", Email = "layla.haddad@takwene.com", Country = "LB" },
            new { Id = ArtistOmarId, Name = "Omar Al-Rashid", Email = "omar.alrashid@takwene.com", Country = "SA" },
            new { Id = ArtistYasmineId, Name = "Yasmine Benali", Email = "yasmine.benali@takwene.com", Country = "MA" }
        );

        // 3. Seed Tracks
        modelBuilder.Entity<Track>().HasData(
            // 3 Draft tracks
            new { Id = Track1Id, Title = "Layali El Qahira", ArtistId = ArtistNourId, Isrc = "EGA012600101", ReleaseDate = new DateOnly(2026, 1, 15), Genre = "Mahraganat", Status = TrackStatus.Draft },
            new { Id = Track2Id, Title = "Beirut Bil Lail", ArtistId = ArtistLaylaId, Isrc = "LBA012600102", ReleaseDate = new DateOnly(2026, 2, 1), Genre = "Arabic Pop", Status = TrackStatus.Draft },
            new { Id = Track3Id, Title = "Sahra Fel Janoub", ArtistId = ArtistOmarId, Isrc = "SAA012600103", ReleaseDate = new DateOnly(2026, 2, 20), Genre = "Khaleeji", Status = TrackStatus.Draft },

            // 3 Submitted tracks
            new { Id = Track4Id, Title = "Nabd El Mahata", ArtistId = ArtistNourId, Isrc = "EGA012600104", ReleaseDate = new DateOnly(2026, 3, 1), Genre = "Shaabi", Status = TrackStatus.Submitted },
            new { Id = Track5Id, Title = "Qalb Men Ward", ArtistId = ArtistLaylaId, Isrc = "LBA012600105", ReleaseDate = new DateOnly(2026, 3, 10), Genre = "Arabic Pop", Status = TrackStatus.Submitted },
            new { Id = Track6Id, Title = "Ghaymat Sheta", ArtistId = ArtistYasmineId, Isrc = "MAA012600106", ReleaseDate = new DateOnly(2026, 3, 25), Genre = "Rai", Status = TrackStatus.Submitted },

            // 4 Distributed tracks
            new { Id = Track7Id, Title = "Darb El Hawa", ArtistId = ArtistNourId, Isrc = "EGA012600107", ReleaseDate = new DateOnly(2025, 11, 1), Genre = "Mahraganat", Status = TrackStatus.Distributed },
            new { Id = Track8Id, Title = "Awtar El Rouh", ArtistId = ArtistLaylaId, Isrc = "LBA012600108", ReleaseDate = new DateOnly(2025, 12, 5), Genre = "Arabic Pop", Status = TrackStatus.Distributed },
            new { Id = Track9Id, Title = "Rimal El Majd", ArtistId = ArtistOmarId, Isrc = "SAA012600109", ReleaseDate = new DateOnly(2026, 1, 1), Genre = "Khaleeji", Status = TrackStatus.Distributed },
            new { Id = Track10Id, Title = "Sawt El Atlas", ArtistId = ArtistYasmineId, Isrc = "MAA012600110", ReleaseDate = new DateOnly(2026, 1, 10), Genre = "Hip-Hop", Status = TrackStatus.Distributed }
        );

        // 4. Seed Distributions
        modelBuilder.Entity<TrackDistribution>().HasData(
            // Distributions for Track 4 (Submitted)
            new { Id = new Guid("019318a0-0031-7000-8000-000000000001"), TrackId = Track4Id, DspId = SpotifyId, SubmittedAt = BaseSeedTime.AddDays(45), Status = DistributionStatus.Pending },
            new { Id = new Guid("019318a0-0031-7000-8000-000000000002"), TrackId = Track4Id, DspId = AppleMusicId, SubmittedAt = BaseSeedTime.AddDays(45), Status = DistributionStatus.Rejected },

            // Distributions for Track 5 (Submitted)
            new { Id = new Guid("019318a0-0032-7000-8000-000000000001"), TrackId = Track5Id, DspId = SpotifyId, SubmittedAt = BaseSeedTime.AddDays(54), Status = DistributionStatus.Pending },
            new { Id = new Guid("019318a0-0032-7000-8000-000000000002"), TrackId = Track5Id, DspId = YouTubeId, SubmittedAt = BaseSeedTime.AddDays(54), Status = DistributionStatus.Pending },

            // Distributions for Track 6 (Submitted)
            new { Id = new Guid("019318a0-0033-7000-8000-000000000001"), TrackId = Track6Id, DspId = AppleMusicId, SubmittedAt = BaseSeedTime.AddDays(69), Status = DistributionStatus.Pending },

            // Distributions for Track 7 (Distributed - Live on Spotify, Apple Music, YouTube)
            new { Id = new Guid("019318a0-0034-7000-8000-000000000001"), TrackId = Track7Id, DspId = SpotifyId, SubmittedAt = BaseSeedTime.AddDays(-75), Status = DistributionStatus.Live },
            new { Id = new Guid("019318a0-0034-7000-8000-000000000002"), TrackId = Track7Id, DspId = AppleMusicId, SubmittedAt = BaseSeedTime.AddDays(-75), Status = DistributionStatus.Live },
            new { Id = new Guid("019318a0-0034-7000-8000-000000000003"), TrackId = Track7Id, DspId = YouTubeId, SubmittedAt = BaseSeedTime.AddDays(-75), Status = DistributionStatus.Live },

            // Distributions for Track 8 (Distributed - Live on Spotify, Apple Music)
            new { Id = new Guid("019318a0-0035-7000-8000-000000000001"), TrackId = Track8Id, DspId = SpotifyId, SubmittedAt = BaseSeedTime.AddDays(-40), Status = DistributionStatus.Live },
            new { Id = new Guid("019318a0-0035-7000-8000-000000000002"), TrackId = Track8Id, DspId = AppleMusicId, SubmittedAt = BaseSeedTime.AddDays(-40), Status = DistributionStatus.Live },

            // Distributions for Track 9 (Distributed - Live on Apple Music, Rejected on YouTube)
            new { Id = new Guid("019318a0-0036-7000-8000-000000000001"), TrackId = Track9Id, DspId = AppleMusicId, SubmittedAt = BaseSeedTime.AddDays(-14), Status = DistributionStatus.Live },
            new { Id = new Guid("019318a0-0036-7000-8000-000000000002"), TrackId = Track9Id, DspId = YouTubeId, SubmittedAt = BaseSeedTime.AddDays(-14), Status = DistributionStatus.Rejected },

            // Distributions for Track 10 (Distributed - Live on Spotify and YouTube)
            new { Id = new Guid("019318a0-0037-7000-8000-000000000001"), TrackId = Track10Id, DspId = SpotifyId, SubmittedAt = BaseSeedTime.AddDays(-5), Status = DistributionStatus.Live },
            new { Id = new Guid("019318a0-0037-7000-8000-000000000002"), TrackId = Track10Id, DspId = YouTubeId, SubmittedAt = BaseSeedTime.AddDays(-5), Status = DistributionStatus.Live }
        );
    }
}
