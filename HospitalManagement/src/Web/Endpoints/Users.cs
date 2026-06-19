using System.Security.Claims;

using HospitalManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();

        groupBuilder.MapGet(GetCurrentUser, "me").RequireAuthorization();
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    [EndpointSummary("Get current user")]
    [EndpointDescription("Returns the signed-in user's email address and assigned roles.")]
    public static async Task<Ok<CurrentUserDto>> GetCurrentUser(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        var email = user?.Email
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.Identity?.Name
            ?? string.Empty;

        IReadOnlyCollection<string> roles = user is not null
            ? [.. await userManager.GetRolesAsync(user)]
            : principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToArray();

        return TypedResults.Ok(new CurrentUserDto(email, roles));
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user by clearing the authentication cookie.")]
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Logout(SignInManager<ApplicationUser> signInManager, [FromBody] object empty)
    {
        if (empty != null)
        {
            await signInManager.SignOutAsync();
            return TypedResults.Ok();
        }

        return TypedResults.Unauthorized();
    }

    public sealed record CurrentUserDto(string Email, IReadOnlyCollection<string> Roles);
}
