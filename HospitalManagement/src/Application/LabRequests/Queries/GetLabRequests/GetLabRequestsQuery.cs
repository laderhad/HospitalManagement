using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabRequests.Queries.GetLabRequests;

[Authorize(Roles = Roles.Administrator)]
public record GetLabRequestsQuery : IRequest<IReadOnlyCollection<LabRequestSummaryDto>>;

public class GetLabRequestsQueryHandler : IRequestHandler<GetLabRequestsQuery, IReadOnlyCollection<LabRequestSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLabRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<LabRequestSummaryDto>> Handle(GetLabRequestsQuery request, CancellationToken cancellationToken)
    {
        return await _context.LabRequests
            .AsNoTracking()
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
