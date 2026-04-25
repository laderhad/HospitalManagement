using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(BeUniqueName)
                .WithMessage("Department name must be unique.");
    }

    private async Task<bool> BeUniqueName(UpdateDepartmentCommand command, string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpper();

        return !await _context.Departments
            .AnyAsync(d => d.Id != command.Id && d.Name.ToUpper() == normalizedName, cancellationToken);
    }
}