using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Departments.Commands.SetDepartmentActiveState;

[Authorize(Roles = Roles.Administrator)]
public record SetDepartmentActiveStateCommand : IRequest
{
    public Guid Id { get; init; }

    public bool IsActive { get; init; }
}

public class SetDepartmentActiveStateCommandHandler : IRequestHandler<SetDepartmentActiveStateCommand>
{
    private readonly IApplicationDbContext _context;

    public SetDepartmentActiveStateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SetDepartmentActiveStateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Departments
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        if (request.IsActive)
        {
            entity.Activate();
        }
        else
        {
            entity.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}