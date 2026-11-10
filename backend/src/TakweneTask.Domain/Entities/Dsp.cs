namespace TakweneTask.Domain.Entities;

public class Dsp
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private Dsp()
    {
    }

    public Dsp(Guid id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
