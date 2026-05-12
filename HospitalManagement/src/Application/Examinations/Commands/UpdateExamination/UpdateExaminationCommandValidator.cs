namespace HospitalManagement.Application.Examinations.Commands.UpdateExamination;

public class UpdateExaminationCommandValidator : AbstractValidator<UpdateExaminationCommand>
{
    public UpdateExaminationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Diagnosis)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Treatment)
            .NotEmpty()
            .MaximumLength(500);
    }
}
