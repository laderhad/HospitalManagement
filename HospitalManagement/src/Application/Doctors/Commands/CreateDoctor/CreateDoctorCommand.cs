using FluentValidation.Results;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.ValueObjects;

namespace HospitalManagement.Application.Doctors.Commands.CreateDoctor;

[Authorize(Roles = Roles.Administrator)]
public record CreateDoctorCommand : IRequest<Guid>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public Guid DepartmentId { get; init; }

    public string ContactNumber { get; init; } = string.Empty;

    public string? Specialty { get; init; }

    public string? Title { get; init; }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreateDoctorCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        var (createResult, userId) = await _identityService.CreateUserAsync(email, request.Password);

        if (!createResult.Succeeded)
        {
            throw ToValidationException(nameof(request.Email), createResult.Errors);
        }

        var roleResult = await _identityService.AddToRoleAsync(userId, Roles.Doctor);

        if (!roleResult.Succeeded)
        {
            await _identityService.DeleteUserAsync(userId);
            throw ToValidationException(nameof(request.Email), roleResult.Errors);
        }

        if (await _context.Doctors.AnyAsync(d => d.ApplicationUserId == userId, cancellationToken))
        {
            await _identityService.DeleteUserAsync(userId);
            throw ToValidationException(nameof(request.Email), ["A doctor profile already exists for this user."]);
        }

        var entity = new Doctor(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.DepartmentId,
            ContactNumber.From(request.ContactNumber.Trim()),
            userId,
            request.Specialty?.Trim(),
            request.Title?.Trim());

        _context.Doctors.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private static HospitalManagement.Application.Common.Exceptions.ValidationException ToValidationException(string propertyName, IEnumerable<string> errors)
    {
        return new HospitalManagement.Application.Common.Exceptions.ValidationException(errors.Select(error => new ValidationFailure(propertyName, error)));
    }
}