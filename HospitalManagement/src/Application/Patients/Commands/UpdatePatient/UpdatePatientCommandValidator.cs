namespace HospitalManagement.Application.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

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

    private static bool BeOnOrBeforeToday(DateTime date)
    {
        return date.Date <= DateTime.UtcNow.Date;
    }
}