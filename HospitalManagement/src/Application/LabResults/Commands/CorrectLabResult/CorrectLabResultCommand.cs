using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabResults.Commands.CorrectLabResult;

[Authorize(Roles = Roles.Administrator)]
public record CorrectLabResultCommand : IRequest
{
    public Guid Id { get; init; }

    public string ResultValue { get; init; } = string.Empty;

    public string Units { get; init; } = string.Empty;

    public string ReferenceRange { get; init; } = string.Empty;

    public string? Notes { get; init; }
}

public class CorrectLabResultCommandHandler : IRequestHandler<CorrectLabResultCommand>
{
    private readonly IApplicationDbContext _context;

    public CorrectLabResultCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CorrectLabResultCommand request, CancellationToken cancellationToken)
    {
        var labResult = await _context.LabResults
            .SingleOrDefaultAsync(lr => lr.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, labResult);

        labResult.Correct(
            request.ResultValue.Trim(),
            request.Units.Trim(),
            request.ReferenceRange.Trim(),
            request.Notes?.Trim());

        await _context.SaveChangesAsync(cancellationToken);
    }
}
