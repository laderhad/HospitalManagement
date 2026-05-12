namespace HospitalManagement.Application.LabResults.Commands.CorrectLabResult;

public class CorrectLabResultCommandValidator : AbstractValidator<CorrectLabResultCommand>
{
    public CorrectLabResultCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.ResultValue)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Units)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ReferenceRange)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
