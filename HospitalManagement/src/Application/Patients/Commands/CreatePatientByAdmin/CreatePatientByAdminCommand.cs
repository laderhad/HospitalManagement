using FluentValidation.Results;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Domain.ValueObjects;

namespace HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;

[Authorize(Roles = Roles.Administrator)]
public record CreatePatientByAdminCommand : IRequest<Guid>
{
    public string ApplicationUserId { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public DateTime DateOfBirth { get; init; }

    public Gender Gender { get; init; }

    public string ContactNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
}

public class CreatePatientByAdminCommandHandler : IRequestHandler<CreatePatientByAdminCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreatePatientByAdminCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(CreatePatientByAdminCommand request, CancellationToken cancellationToken)
    {
        var applicationUserId = request.ApplicationUserId.Trim();

        if (await _identityService.GetUserNameAsync(applicationUserId) is null)
        {
            throw ToValidationException(nameof(request.ApplicationUserId), ["User was not found."]);
        }

        if (await _context.Patients.AnyAsync(p => p.ApplicationUserId == applicationUserId, cancellationToken))
        {
            throw ToValidationException(nameof(request.ApplicationUserId), ["A patient profile already exists for this user."]);
        }

        var roleResult = await _identityService.AddToRoleAsync(applicationUserId, Roles.Patient);

        if (!roleResult.Succeeded)
        {
            throw ToValidationException(nameof(request.ApplicationUserId), roleResult.Errors);
        }

        var entity = new Patient(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            NormalizeToUtc(request.DateOfBirth),
            request.Gender,
            ContactNumber.From(request.ContactNumber.Trim()),
            PostalAddress.From(request.Address.Trim()),
            applicationUserId);

        _context.Patients.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
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

    private static HospitalManagement.Application.Common.Exceptions.ValidationException ToValidationException(string propertyName, IEnumerable<string> errors)
    {
        return new HospitalManagement.Application.Common.Exceptions.ValidationException(errors.Select(error => new ValidationFailure(propertyName, error)));
    }
}