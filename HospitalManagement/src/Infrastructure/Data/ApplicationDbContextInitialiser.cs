using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Domain.ValueObjects;
using HospitalManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        var defaultRoles = new[]
        {
            new IdentityRole(Roles.Administrator),
            new IdentityRole(Roles.Doctor),
            new IdentityRole(Roles.Patient)
        };

        foreach (var role in defaultRoles)
        {
            if (_roleManager.Roles.All(r => r.Name != role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }

        var administrator = await EnsureUserAsync("administrator@localhost", "Administrator1!", Roles.Administrator);
        var doctorUser = await EnsureUserAsync("doctor@localhost", "Doctor1!", Roles.Doctor);
        var patientUser = await EnsureUserAsync("patient@localhost", "Patient1!", Roles.Patient);

        var cardiology = await EnsureDepartmentAsync("Cardiology");
        await EnsureDoctorAsync(doctorUser.Id, "Demo", "Doctor", cardiology.Id, "555-0101", "Cardiology", "Specialist");
        await EnsurePatientAsync(patientUser.Id, "Demo", "Patient", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc), Gender.Unknown, "555-0102", "Demo address");

        await _context.SaveChangesAsync();
    }

    private async Task<ApplicationUser> EnsureUserAsync(string email, string password, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create seed user '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign role '{role}' to seed user '{email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        return user;
    }

    private async Task<Department> EnsureDepartmentAsync(string name)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == name);

        if (department != null)
        {
            return department;
        }

        department = new Department(name);
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return department;
    }

    private async Task EnsureDoctorAsync(string applicationUserId, string firstName, string lastName, Guid departmentId, string contactNumber, string? specialty, string? title)
    {
        if (await _context.Doctors.AnyAsync(d => d.ApplicationUserId == applicationUserId))
        {
            return;
        }

        _context.Doctors.Add(new Doctor(
            firstName,
            lastName,
            departmentId,
            ContactNumber.From(contactNumber),
            applicationUserId,
            specialty,
            title));
    }

    private async Task EnsurePatientAsync(string applicationUserId, string firstName, string lastName, DateTime dateOfBirth, Gender gender, string contactNumber, string address)
    {
        if (await _context.Patients.AnyAsync(p => p.ApplicationUserId == applicationUserId))
        {
            return;
        }

        _context.Patients.Add(new Patient(
            firstName,
            lastName,
            dateOfBirth,
            gender,
            ContactNumber.From(contactNumber),
            PostalAddress.From(address),
            applicationUserId));
    }
}
