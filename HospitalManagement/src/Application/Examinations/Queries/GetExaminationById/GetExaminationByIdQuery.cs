using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.Examinations.Queries.GetExaminationById;

[Authorize]
public record GetExaminationByIdQuery(Guid Id) : IRequest<ExaminationDetailsDto>;

public class GetExaminationByIdQueryHandler : IRequestHandler<GetExaminationByIdQuery, ExaminationDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetExaminationByIdQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<ExaminationDetailsDto> Handle(GetExaminationByIdQuery request, CancellationToken cancellationToken)
    {
        var examination = await _context.Examinations
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(e => new
            {
                Details = new ExaminationDetailsDto
                {
                    Id = e.Id,
                    AppointmentId = e.AppointmentId,
                    AppointmentDate = e.Appointment!.AppointmentDate,
                    PatientId = e.Appointment.PatientId,
                    PatientFullName = e.Appointment.Patient!.FirstName + " " + e.Appointment.Patient.LastName,
                    DoctorId = e.Appointment.DoctorId,
                    DoctorFullName = e.Appointment.Doctor!.FirstName + " " + e.Appointment.Doctor.LastName,
                    Diagnosis = e.Diagnosis,
                    Treatment = e.Treatment,
                    PrescriptionIds = e.Prescriptions.Select(p => p.Id).ToList(),
                    LabRequestIds = e.LabRequests.Select(lr => lr.Id).ToList()
                },
                PatientId = e.Appointment.PatientId,
                DoctorId = e.Appointment.DoctorId
            })
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, examination);

        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return examination.Details;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (doctorId == examination.DoctorId)
            {
                return examination.Details;
            }
        }

        if (ClinicalAccessHelper.IsPatient(_user))
        {
            var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

            if (patientId == examination.PatientId)
            {
                return examination.Details;
            }
        }

        throw new ForbiddenAccessException();
    }
}
