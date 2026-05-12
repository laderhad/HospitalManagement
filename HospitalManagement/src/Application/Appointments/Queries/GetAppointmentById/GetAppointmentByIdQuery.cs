using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.Appointments.Queries.GetAppointmentById;

[Authorize]
public record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDetailsDto>;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetAppointmentByIdQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<AppointmentDetailsDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.Status,
                a.PatientId,
                PatientFullName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : string.Empty,
                a.DoctorId,
                DoctorFullName = a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : string.Empty,
                DepartmentId = a.Doctor != null ? a.Doctor.DepartmentId : Guid.Empty,
                DepartmentName = a.Doctor != null && a.Doctor.Department != null ? a.Doctor.Department.Name : string.Empty
            })
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, appointment);

        if (!ClinicalAccessHelper.IsAdministrator(_user))
        {
            if (ClinicalAccessHelper.IsDoctor(_user))
            {
                var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

                if (appointment.DoctorId != doctorId)
                {
                    throw new ForbiddenAccessException();
                }
            }
            else if (ClinicalAccessHelper.IsPatient(_user))
            {
                var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

                if (appointment.PatientId != patientId)
                {
                    throw new ForbiddenAccessException();
                }
            }
            else
            {
                throw new ForbiddenAccessException();
            }
        }

        return new AppointmentDetailsDto(
            appointment.Id,
            appointment.AppointmentDate,
            appointment.Status,
            appointment.PatientId,
            appointment.PatientFullName,
            appointment.DoctorId,
            appointment.DoctorFullName,
            appointment.DepartmentId,
            appointment.DepartmentName);
    }
}
