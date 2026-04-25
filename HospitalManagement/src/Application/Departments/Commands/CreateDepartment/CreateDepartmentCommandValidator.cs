using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(BeUniqueName)
                .WithMessage("Department name must be unique.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpper();

        return !await _context.Departments
            .AnyAsync(d => d.Name.ToUpper() == normalizedName, cancellationToken);
    }
}