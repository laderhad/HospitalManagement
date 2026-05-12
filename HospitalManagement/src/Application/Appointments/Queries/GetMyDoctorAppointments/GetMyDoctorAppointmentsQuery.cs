using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Appointments.Queries.GetMyDoctorAppointments;

[Authorize(Roles = Roles.Doctor)]
public record GetMyDoctorAppointmentsQuery : IRequest<IReadOnlyCollection<GetAppointments.AppointmentSummaryDto>>;

public class GetMyDoctorAppointmentsQueryHandler : IRequestHandler<GetMyDoctorAppointmentsQuery, IReadOnlyCollection<GetAppointments.AppointmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyDoctorAppointmentsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<GetAppointments.AppointmentSummaryDto>> Handle(GetMyDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new GetAppointments.AppointmentSummaryDto(
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
