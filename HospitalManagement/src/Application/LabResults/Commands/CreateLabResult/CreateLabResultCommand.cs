using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabResults.Commands.CreateLabResult;

[Authorize(Roles = Roles.Administrator)]
public record CreateLabResultCommand : IRequest<Guid>
{
    public Guid LabRequestItemId { get; init; }

    public string ResultValue { get; init; } = string.Empty;

    public string Units { get; init; } = string.Empty;

    public string ReferenceRange { get; init; } = string.Empty;

    public string? Notes { get; init; }
}

public class CreateLabResultCommandHandler : IRequestHandler<CreateLabResultCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateLabResultCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLabResultCommand request, CancellationToken cancellationToken)
    {
        var labRequestItem = await _context.LabRequestItems
            .Include(item => item.Results)
            .SingleOrDefaultAsync(item => item.Id == request.LabRequestItemId, cancellationToken);

        Guard.Against.NotFound(request.LabRequestItemId, labRequestItem);

        var result = labRequestItem.AddResult(
            request.ResultValue.Trim(),
            request.Units.Trim(),
            request.ReferenceRange.Trim(),
            request.Notes?.Trim());

        await _context.SaveChangesAsync(cancellationToken);

        return result.Id;
    }
}
