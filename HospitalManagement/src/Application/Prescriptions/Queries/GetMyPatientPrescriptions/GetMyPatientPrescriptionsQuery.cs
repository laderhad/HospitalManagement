using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Application.Prescriptions.Queries.GetPrescriptions;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Prescriptions.Queries.GetMyPatientPrescriptions;

[Authorize(Roles = Roles.Patient)]
public record GetMyPatientPrescriptionsQuery : IRequest<IReadOnlyCollection<PrescriptionSummaryDto>>;

public class GetMyPatientPrescriptionsQueryHandler : IRequestHandler<GetMyPatientPrescriptionsQuery, IReadOnlyCollection<PrescriptionSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyPatientPrescriptionsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<PrescriptionSummaryDto>> Handle(GetMyPatientPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

        return await _context.Prescriptions
            .AsNoTracking()
            .Where(p => p.Examination!.Appointment!.PatientId == patientId)
            .OrderByDescending(p => p.Created)
            .Select(p => new PrescriptionSummaryDto
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
            })
            .ToListAsync(cancellationToken);
    }
}
