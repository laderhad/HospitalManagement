namespace HospitalManagement.Application.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.AppointmentDate)
            .Must(date => CreateAppointment.CreateAppointmentCommandHandler.NormalizeToUtc(date) > DateTime.UtcNow)
            .WithMessage("Appointment date must be in the future.");
    }
}
