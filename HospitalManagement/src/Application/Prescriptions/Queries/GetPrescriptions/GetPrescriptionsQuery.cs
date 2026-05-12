using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Prescriptions.Queries.GetPrescriptions;

[Authorize(Roles = Roles.Administrator)]
public record GetPrescriptionsQuery : IRequest<IReadOnlyCollection<PrescriptionSummaryDto>>;

public class GetPrescriptionsQueryHandler : IRequestHandler<GetPrescriptionsQuery, IReadOnlyCollection<PrescriptionSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPrescriptionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<PrescriptionSummaryDto>> Handle(GetPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Prescriptions
            .AsNoTracking()
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
