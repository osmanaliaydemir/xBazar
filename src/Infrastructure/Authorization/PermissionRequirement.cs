using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public string? Resource { get; }

    public PermissionRequirement(string permission, string? resource = null)
    {
        Permission = permission;
        Resource = resource;
    }
}
