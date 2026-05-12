using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Examinations.Queries.GetExaminations;

[Authorize(Roles = Roles.Administrator)]
public record GetExaminationsQuery : IRequest<IReadOnlyCollection<ExaminationSummaryDto>>;

public class GetExaminationsQueryHandler : IRequestHandler<GetExaminationsQuery, IReadOnlyCollection<ExaminationSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExaminationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ExaminationSummaryDto>> Handle(GetExaminationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Examinations
            .AsNoTracking()
            .OrderByDescending(e => e.Created)
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
