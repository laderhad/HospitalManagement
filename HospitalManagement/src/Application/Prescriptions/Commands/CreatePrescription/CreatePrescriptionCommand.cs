using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.Prescriptions.Commands.CreatePrescription;

[Authorize]
public record CreatePrescriptionCommand : IRequest<Guid>
{
    public Guid ExaminationId { get; init; }

    public string MedicationName { get; init; } = string.Empty;

    public string Dosage { get; init; } = string.Empty;

    public string Frequency { get; init; } = string.Empty;

    public int DurationDays { get; init; }

    public string? Notes { get; init; }
}

public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreatePrescriptionCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var examination = await _context.Examinations
            .Include(e => e.Appointment)
            .Include(e => e.Prescriptions)
            .SingleOrDefaultAsync(e => e.Id == request.ExaminationId, cancellationToken);

        Guard.Against.NotFound(request.ExaminationId, examination);

        await EnsureCanManageAsync(examination.Appointment!.DoctorId, cancellationToken);

        var prescription = examination.AddPrescription(
            request.MedicationName.Trim(),
            request.Dosage.Trim(),
            request.Frequency.Trim(),
            request.DurationDays,
            request.Notes?.Trim());

        await _context.SaveChangesAsync(cancellationToken);

        return prescription.Id;
    }

    private async Task EnsureCanManageAsync(Guid doctorId, CancellationToken cancellationToken)
    {
        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var currentDoctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (currentDoctorId == doctorId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException();
    }
}
