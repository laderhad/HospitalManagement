using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.ValueObjects;

namespace HospitalManagement.Application.Doctors.Commands.UpdateDoctor;

[Authorize(Roles = Roles.Administrator)]
public record UpdateDoctorCommand : IRequest
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public Guid DepartmentId { get; init; }

    public string ContactNumber { get; init; } = string.Empty;

    public string? Specialty { get; init; }

    public string? Title { get; init; }
}

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDoctorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Doctors
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Rename(request.FirstName.Trim(), request.LastName.Trim());
        entity.ChangeDepartment(request.DepartmentId);
        entity.UpdateContactNumber(ContactNumber.From(request.ContactNumber.Trim()));
        entity.UpdateProfessionalDetails(request.Specialty?.Trim(), request.Title?.Trim());

        await _context.SaveChangesAsync(cancellationToken);
    }
}