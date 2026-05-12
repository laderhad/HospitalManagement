using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Application.Examinations.Queries.GetExaminations;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Examinations.Queries.GetMyPatientExaminations;

[Authorize(Roles = Roles.Patient)]
public record GetMyPatientExaminationsQuery : IRequest<IReadOnlyCollection<ExaminationSummaryDto>>;

public class GetMyPatientExaminationsQueryHandler : IRequestHandler<GetMyPatientExaminationsQuery, IReadOnlyCollection<ExaminationSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyPatientExaminationsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<ExaminationSummaryDto>> Handle(GetMyPatientExaminationsQuery request, CancellationToken cancellationToken)
    {
        var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

        return await _context.Examinations
            .AsNoTracking()
            .Where(e => e.Appointment!.PatientId == patientId)
            .OrderByDescending(e => e.Appointment!.AppointmentDate)
            .Select(e => new ExaminationSummaryDto
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
                PrescriptionCount = e.Prescriptions.Count,
                LabRequestCount = e.LabRequests.Count
            })
            .ToListAsync(cancellationToken);
    }
}
