using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Departments.Commands.UpdateDepartment;

[Authorize(Roles = Roles.Administrator)]
public record UpdateDepartmentCommand : IRequest
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Departments
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Rename(request.Name.Trim());

        await _context.SaveChangesAsync(cancellationToken);
    }
}