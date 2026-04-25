using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Departments.Commands.CreateDepartment;

[Authorize(Roles = Roles.Administrator)]
public record CreateDepartmentCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Department(request.Name.Trim());

        _context.Departments.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}