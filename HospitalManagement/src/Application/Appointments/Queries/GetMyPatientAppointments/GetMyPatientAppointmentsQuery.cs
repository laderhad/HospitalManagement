using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Appointments.Queries.GetMyPatientAppointments;

[Authorize(Roles = Roles.Patient)]
public record GetMyPatientAppointmentsQuery : IRequest<IReadOnlyCollection<GetAppointments.AppointmentSummaryDto>>;

public class GetMyPatientAppointmentsQueryHandler : IRequestHandler<GetMyPatientAppointmentsQuery, IReadOnlyCollection<GetAppointments.AppointmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyPatientAppointmentsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<GetAppointments.AppointmentSummaryDto>> Handle(GetMyPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
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
