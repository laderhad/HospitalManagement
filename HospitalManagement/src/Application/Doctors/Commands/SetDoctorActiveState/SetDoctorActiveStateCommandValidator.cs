namespace HospitalManagement.Application.Doctors.Commands.SetDoctorActiveState;

public class SetDoctorActiveStateCommandValidator : AbstractValidator<SetDoctorActiveStateCommand>
{
    public SetDoctorActiveStateCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();
    }
}