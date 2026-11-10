namespace TakweneTask.Application.DTOs.Artists;

public class CreateArtistRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
