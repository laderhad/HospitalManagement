using System.Security.Claims;

using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Domain.ValueObjects;
using HospitalManagement.Infrastructure.Data;
using HospitalManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterPatient, "register-patient");
        groupBuilder.MapIdentityApi<ApplicationUser>();

        groupBuilder.MapGet(GetCurrentUser, "me").RequireAuthorization();
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    [EndpointSummary("Register patient")]
    [EndpointDescription("Creates a patient identity account, assigns the Patient role, and creates the patient profile.")]
    public static async Task<Results<Ok, ValidationProblem>> RegisterPatient(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        var fieldErrors = ValidatePatientRegistration(request);

        if (fieldErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(fieldErrors);
        }

        var executionStrategy = context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async Task<Results<Ok, ValidationProblem>> () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            var email = request.Email.Trim();
            var user = new ApplicationUser { UserName = email, Email = email };
            var createResult = await userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                return TypedResults.ValidationProblem(ToErrorDictionary(createResult));
            }

            if (!await roleManager.RoleExistsAsync(Roles.Patient))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(Roles.Patient));

                if (!createRoleResult.Succeeded)
                {
                    return TypedResults.ValidationProblem(ToErrorDictionary(createRoleResult));
                }
            }

            var roleResult = await userManager.AddToRoleAsync(user, Roles.Patient);

            if (!roleResult.Succeeded)
            {
                return TypedResults.ValidationProblem(ToErrorDictionary(roleResult));
            }

            context.Patients.Add(new Patient(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                NormalizeToUtc(request.DateOfBirth),
                request.Gender,
                ContactNumber.From(request.ContactNumber.Trim()),
                PostalAddress.From(request.Address.Trim()),
                user.Id));

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return TypedResults.Ok();
        });
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

    public sealed record RegisterPatientRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        Gender Gender,
        string ContactNumber,
        string Address);

    private static Dictionary<string, string[]> ValidatePatientRegistration(RegisterPatientRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        AddRequired(errors, nameof(request.Email), request.Email);
        AddRequired(errors, nameof(request.Password), request.Password);
        AddRequired(errors, nameof(request.FirstName), request.FirstName);
        AddRequired(errors, nameof(request.LastName), request.LastName);
        AddRequired(errors, nameof(request.ContactNumber), request.ContactNumber);
        AddRequired(errors, nameof(request.Address), request.Address);

        if (request.DateOfBirth == default)
        {
            errors[nameof(request.DateOfBirth)] = ["Date of birth is required."];
        }
        else if (request.DateOfBirth.Date > DateTime.UtcNow.Date)
        {
            errors[nameof(request.DateOfBirth)] = ["Date of birth cannot be in the future."];
        }

        return errors;
    }

    private static void AddRequired(IDictionary<string, string[]> errors, string propertyName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[propertyName] = [$"{propertyName} is required."];
        }
    }

    private static Dictionary<string, string[]> ToErrorDictionary(IdentityResult result)
    {
        return result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());
    }

    private static DateTime NormalizeToUtc(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }
}
