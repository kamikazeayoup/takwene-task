namespace TakweneTask.Application.DTOs.Tracks;

public class CreateTrackRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid ArtistId { get; set; }
    public string Isrc { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string Genre { get; set; } = string.Empty;
}
