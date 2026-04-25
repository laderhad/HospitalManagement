using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;

public class CreatePatientByAdminCommandValidator : AbstractValidator<CreatePatientByAdminCommand>
{
    private readonly IApplicationDbContext _context;

    public CreatePatientByAdminCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.ApplicationUserId)
            .NotEmpty()
            .MaximumLength(450)
            .MustAsync(BeUniqueApplicationUserId)
                .WithMessage("A patient profile already exists for this user.");

        RuleFor(v => v.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.DateOfBirth)
            .NotEqual(default(DateTime))
            .Must(BeOnOrBeforeToday)
                .WithMessage("Date of birth cannot be in the future.");

        RuleFor(v => v.ContactNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(v => v.Address)
            .NotEmpty()
            .MaximumLength(500);
    }

    private async Task<bool> BeUniqueApplicationUserId(string applicationUserId, CancellationToken cancellationToken)
    {
        return !await _context.Patients.AnyAsync(p => p.ApplicationUserId == applicationUserId.Trim(), cancellationToken);
    }

    private static bool BeOnOrBeforeToday(DateTime date)
    {
        return date.Date <= DateTime.UtcNow.Date;
    }
}