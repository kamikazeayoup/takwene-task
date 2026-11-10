namespace TakweneTask.Domain.Entities;

public class Artist
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    public ICollection<Track> Tracks { get; private set; } = new List<Track>();

    private Artist()
    {
    }

    public Artist(Guid id, string name, string email, string country)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }
}
