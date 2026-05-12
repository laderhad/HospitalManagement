using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.LabResults.Commands.CreateLabResult;

public class CreateLabResultCommandValidator : AbstractValidator<CreateLabResultCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateLabResultCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.LabRequestItemId)
            .NotEmpty()
            .MustAsync(LabRequestItemExists).WithMessage("Lab request item was not found.");

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

    private async Task<bool> LabRequestItemExists(Guid labRequestItemId, CancellationToken cancellationToken)
    {
        return await _context.LabRequestItems.AnyAsync(item => item.Id == labRequestItemId, cancellationToken);
    }
}
