using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Patients.Queries.GetPatients;

[Authorize(Roles = Roles.Administrator)]
public record GetPatientsQuery : IRequest<IReadOnlyCollection<PatientSummaryDto>>;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, IReadOnlyCollection<PatientSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<PatientSummaryDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PatientSummaryDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.Gender,
                p.ApplicationUserId))
            .ToListAsync(cancellationToken);
    }
}