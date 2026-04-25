using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Enums;
using HospitalManagement.Domain.ValueObjects;

namespace HospitalManagement.Application.Patients.Commands.UpdatePatient;

[Authorize(Roles = Roles.Administrator)]
public record UpdatePatientCommand : IRequest
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public DateTime DateOfBirth { get; init; }

    public Gender Gender { get; init; }

    public string ContactNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
}

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePatientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Patients
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

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