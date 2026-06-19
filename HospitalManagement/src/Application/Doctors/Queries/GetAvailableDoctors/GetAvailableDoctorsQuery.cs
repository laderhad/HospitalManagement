using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Application.Doctors.Queries.GetDoctors;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Doctors.Queries.GetAvailableDoctors;

[Authorize(Roles = Roles.Administrator + "," + Roles.Patient)]
public record GetAvailableDoctorsQuery : IRequest<IReadOnlyCollection<DoctorSummaryDto>>;

public class GetAvailableDoctorsQueryHandler : IRequestHandler<GetAvailableDoctorsQuery, IReadOnlyCollection<DoctorSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DoctorSummaryDto>> Handle(GetAvailableDoctorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Where(d => d.IsActive)
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
