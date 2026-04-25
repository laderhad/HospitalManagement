using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Domain.ValueObjects;

namespace HospitalManagement.Application.Patients.Commands.UpdateMyPatientProfile;

[Authorize(Roles = Roles.Patient)]
public record UpdateMyPatientProfileCommand : IRequest
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public DateTime DateOfBirth { get; init; }

    public Gender Gender { get; init; }

    public string ContactNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
}

public class UpdateMyPatientProfileCommandHandler : IRequestHandler<UpdateMyPatientProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateMyPatientProfileCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateMyPatientProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = Guard.Against.Null(_user.Id);

        var entity = await _context.Patients
            .SingleOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);

        Guard.Against.NotFound(userId, entity);

        entity.UpdatePersonalDetails(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            NormalizeToUtc(request.DateOfBirth),
            request.Gender);

        entity.UpdateContactDetails(
            ContactNumber.From(request.ContactNumber.Trim()),
            PostalAddress.From(request.Address.Trim()));

        await _context.SaveChangesAsync(cancellationToken);
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