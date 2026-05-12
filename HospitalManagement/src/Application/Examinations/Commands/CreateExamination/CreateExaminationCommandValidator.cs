using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Examinations.Commands.CreateExamination;

public class CreateExaminationCommandValidator : AbstractValidator<CreateExaminationCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateExaminationCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .MustAsync(AppointmentExists).WithMessage("Appointment was not found.");

        RuleFor(x => x.Diagnosis)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Treatment)
            .NotEmpty()
            .MaximumLength(500);
    }

    private async Task<bool> AppointmentExists(Guid appointmentId, CancellationToken cancellationToken)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == appointmentId, cancellationToken);
    }
}
