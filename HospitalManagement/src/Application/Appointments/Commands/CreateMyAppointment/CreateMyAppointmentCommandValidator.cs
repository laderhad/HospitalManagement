using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Appointments.Commands.CreateMyAppointment;

public class CreateMyAppointmentCommandValidator : AbstractValidator<CreateMyAppointmentCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateMyAppointmentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.DoctorId)
            .NotEmpty()
            .MustAsync(DoctorExists)
            .WithMessage("Doctor was not found.");

        RuleFor(v => v.AppointmentDate)
            .Must(date => CreateAppointment.CreateAppointmentCommandHandler.NormalizeToUtc(date) > DateTime.UtcNow)
            .WithMessage("Appointment date must be in the future.");
    }

    private Task<bool> DoctorExists(Guid doctorId, CancellationToken cancellationToken) =>
        _context.Doctors.AnyAsync(d => d.Id == doctorId, cancellationToken);
}
