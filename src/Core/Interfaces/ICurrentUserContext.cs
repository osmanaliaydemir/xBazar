using System.Security.Claims;

namespace Core.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
