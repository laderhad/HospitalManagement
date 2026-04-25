using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateDoctorCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(v => v.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(v => v.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.DepartmentId)
            .NotEmpty()
            .MustAsync(DepartmentExists)
                .WithMessage("Department was not found.");

        RuleFor(v => v.ContactNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(v => v.Specialty)
            .MaximumLength(100);

        RuleFor(v => v.Title)
            .MaximumLength(100);
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        return await _context.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
    }
}