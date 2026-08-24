using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Somo.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected string? CurrentUserId => User.FindFirst("id")?.Value;

    protected IEnumerable<string> CurrentRoles =>
        User.FindAll(ClaimTypes.Role).Select(c => c.Value);
}
