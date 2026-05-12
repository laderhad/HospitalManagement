using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.Prescriptions.Queries.GetPrescriptionById;

[Authorize]
public record GetPrescriptionByIdQuery(Guid Id) : IRequest<PrescriptionDetailsDto>;

public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery, PrescriptionDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetPrescriptionByIdQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<PrescriptionDetailsDto> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var prescription = await _context.Prescriptions
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new
            {
                Details = new PrescriptionDetailsDto
                {
                    Id = p.Id,
                    ExaminationId = p.ExaminationId,
                    AppointmentId = p.Examination!.AppointmentId,
                    PatientId = p.Examination.Appointment!.PatientId,
                    PatientFullName = p.Examination.Appointment.Patient!.FirstName + " " + p.Examination.Appointment.Patient.LastName,
                    DoctorId = p.Examination.Appointment.DoctorId,
                    DoctorFullName = p.Examination.Appointment.Doctor!.FirstName + " " + p.Examination.Appointment.Doctor.LastName,
                    MedicationName = p.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    DurationDays = p.DurationDays,
                    Notes = p.Notes
                },
                PatientId = p.Examination.Appointment!.PatientId,
                DoctorId = p.Examination.Appointment.DoctorId
            })
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, prescription);

        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return prescription.Details;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (doctorId == prescription.DoctorId)
            {
                return prescription.Details;
            }
        }

        if (ClinicalAccessHelper.IsPatient(_user))
        {
            var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

            if (patientId == prescription.PatientId)
            {
                return prescription.Details;
            }
        }

        throw new ForbiddenAccessException();
    }
}
