using HospitalManagement.Application.Common.Interfaces;

namespace HospitalManagement.Application.LabRequests.Commands.CreateLabRequest;

public class CreateLabRequestCommandValidator : AbstractValidator<CreateLabRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateLabRequestCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ExaminationId)
            .NotEmpty()
            .MustAsync(ExaminationExists).WithMessage("Examination was not found.");

        RuleFor(x => x.TestNames)
            .NotEmpty().WithMessage("At least one lab test is required.");

        RuleForEach(x => x.TestNames)
            .NotEmpty()
            .MaximumLength(200);
    }

    private async Task<bool> ExaminationExists(Guid examinationId, CancellationToken cancellationToken)
    {
        return await _context.Examinations.AnyAsync(e => e.Id == examinationId, cancellationToken);
    }
}
