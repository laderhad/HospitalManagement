using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateAppointmentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.PatientId)
            .NotEmpty()
            .MustAsync(PatientExists)
            .WithMessage("Patient was not found.");

        RuleFor(v => v.DoctorId)
            .NotEmpty()
            .MustAsync(DoctorExists)
            .WithMessage("Doctor was not found.");

        RuleFor(v => v.AppointmentDate)
            .Must(date => CreateAppointmentCommandHandler.NormalizeToUtc(date) > DateTime.UtcNow)
            .WithMessage("Appointment date must be in the future.");
    }

    private Task<bool> PatientExists(Guid patientId, CancellationToken cancellationToken) =>
        _context.Patients.AnyAsync(p => p.Id == patientId, cancellationToken);

    private Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken) =>
        _context.Doctors.AnyAsync(d => d.Id == doctorId, cancellationToken);
}
