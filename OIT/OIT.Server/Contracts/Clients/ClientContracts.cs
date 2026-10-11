using System.ComponentModel.DataAnnotations;

namespace OIT.Server.Contracts.Clients;

public record ClientInput(
    [Required, StringLength(200)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email);
public record ClientUpdate(
    [Required, StringLength(200)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    bool IsActive, Guid Version);
public record ClientView(Guid Id, string Name, string Email, bool IsActive, Guid Version);
