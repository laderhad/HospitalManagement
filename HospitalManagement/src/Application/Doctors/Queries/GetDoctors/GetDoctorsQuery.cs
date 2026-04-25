using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Doctors.Queries.GetDoctors;

[Authorize(Roles = Roles.Administrator)]
public record GetDoctorsQuery : IRequest<IReadOnlyCollection<DoctorSummaryDto>>;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, IReadOnlyCollection<DoctorSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DoctorSummaryDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .Select(d => new DoctorSummaryDto(
                d.Id,
                d.FirstName,
                d.LastName,
                d.DepartmentId,
                d.Department != null ? d.Department.Name : string.Empty,
                d.IsActive))
            .ToListAsync(cancellationToken);
    }
}