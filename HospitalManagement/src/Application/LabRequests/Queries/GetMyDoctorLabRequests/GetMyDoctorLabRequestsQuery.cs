using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Application.LabRequests.Queries.GetLabRequests;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabRequests.Queries.GetMyDoctorLabRequests;

[Authorize(Roles = Roles.Doctor)]
public record GetMyDoctorLabRequestsQuery : IRequest<IReadOnlyCollection<LabRequestSummaryDto>>;

public class GetMyDoctorLabRequestsQueryHandler : IRequestHandler<GetMyDoctorLabRequestsQuery, IReadOnlyCollection<LabRequestSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyDoctorLabRequestsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<LabRequestSummaryDto>> Handle(GetMyDoctorLabRequestsQuery request, CancellationToken cancellationToken)
    {
        var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

        return await _context.LabRequests
            .AsNoTracking()
            .Where(lr => lr.Examination!.Appointment!.DoctorId == doctorId)
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
