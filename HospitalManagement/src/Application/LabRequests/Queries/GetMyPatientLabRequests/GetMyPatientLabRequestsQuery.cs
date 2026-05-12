using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Application.LabRequests.Queries.GetLabRequests;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabRequests.Queries.GetMyPatientLabRequests;

[Authorize(Roles = Roles.Patient)]
public record GetMyPatientLabRequestsQuery : IRequest<IReadOnlyCollection<LabRequestSummaryDto>>;

public class GetMyPatientLabRequestsQueryHandler : IRequestHandler<GetMyPatientLabRequestsQuery, IReadOnlyCollection<LabRequestSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyPatientLabRequestsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<LabRequestSummaryDto>> Handle(GetMyPatientLabRequestsQuery request, CancellationToken cancellationToken)
    {
        var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

        return await _context.LabRequests
            .AsNoTracking()
            .Where(lr => lr.Examination!.Appointment!.PatientId == patientId)
            .OrderByDescending(lr => lr.RequestDate)
            .Select(lr => new LabRequestSummaryDto
            {
                Id = lr.Id,
                ExaminationId = lr.ExaminationId,
                AppointmentId = lr.Examination!.AppointmentId,
                PatientId = lr.Examination.Appointment!.PatientId,
                PatientFullName = lr.Examination.Appointment.Patient!.FirstName + " " + lr.Examination.Appointment.Patient.LastName,
                DoctorId = lr.Examination.Appointment.DoctorId,
                DoctorFullName = lr.Examination.Appointment.Doctor!.FirstName + " " + lr.Examination.Appointment.Doctor.LastName,
                RequestDate = lr.RequestDate,
                ItemCount = lr.Items.Count,
                ResultCount = lr.Items.SelectMany(item => item.Results).Count()
            })
            .ToListAsync(cancellationToken);
    }
}
