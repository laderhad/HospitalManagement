using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Departments.Queries.GetDepartments;

[Authorize(Roles = Roles.Administrator)]
public record GetDepartmentsQuery : IRequest<IReadOnlyCollection<DepartmentSummaryDto>>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, IReadOnlyCollection<DepartmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DepartmentSummaryDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentSummaryDto(d.Id, d.Name, d.IsActive))
            .ToListAsync(cancellationToken);
    }
}