namespace OIT.Server.Domain.Clients;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string NormalizedEmail { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public Guid Version { get; set; } = Guid.NewGuid();
}
