using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    private readonly IApplicationDbContext _context;

    public CreatePrescriptionCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ExaminationId)
            .NotEmpty()
            .MustAsync(ExaminationExists).WithMessage("Examination was not found.");

        RuleFor(x => x.MedicationName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dosage)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Frequency)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }

    private async Task<bool> ExaminationExists(Guid examinationId, CancellationToken cancellationToken)
    {
        return await _context.Examinations.AnyAsync(e => e.Id == examinationId, cancellationToken);
    }
}
