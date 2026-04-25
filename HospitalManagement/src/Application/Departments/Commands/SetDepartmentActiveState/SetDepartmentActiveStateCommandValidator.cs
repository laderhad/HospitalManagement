namespace HospitalManagement.Application.Departments.Commands.SetDepartmentActiveState;

public class SetDepartmentActiveStateCommandValidator : AbstractValidator<SetDepartmentActiveStateCommand>
{
    public SetDepartmentActiveStateCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();
    }
}