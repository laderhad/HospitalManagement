using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Appointments.Queries.GetAppointments;

[Authorize(Roles = Roles.Administrator)]
public record GetAppointmentsQuery : IRequest<IReadOnlyCollection<AppointmentSummaryDto>>;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, IReadOnlyCollection<AppointmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<AppointmentSummaryDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .AsNoTracking()
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentSummaryDto(
                a.Id,
                a.AppointmentDate,
                a.Status,
                a.PatientId,
                a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : string.Empty,
                a.DoctorId,
                a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : string.Empty))
            .ToListAsync(cancellationToken);
    }
}
