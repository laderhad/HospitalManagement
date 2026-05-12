using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabResults.Queries.GetLabResults;

[Authorize(Roles = Roles.Administrator)]
public record GetLabResultsQuery : IRequest<IReadOnlyCollection<LabResultSummaryDto>>;

public class GetLabResultsQueryHandler : IRequestHandler<GetLabResultsQuery, IReadOnlyCollection<LabResultSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLabResultsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<LabResultSummaryDto>> Handle(GetLabResultsQuery request, CancellationToken cancellationToken)
    {
        return await _context.LabResults
            .AsNoTracking()
            .OrderByDescending(lr => lr.ResultDate)
            .Select(lr => new LabResultSummaryDto
            {
                Id = lr.Id,
                LabRequestId = lr.LabRequestItem!.LabRequestId,
                LabRequestItemId = lr.LabRequestItemId,
                ExaminationId = lr.LabRequestItem.LabRequest!.ExaminationId,
                AppointmentId = lr.LabRequestItem.LabRequest.Examination!.AppointmentId,
                PatientId = lr.LabRequestItem.LabRequest.Examination.Appointment!.PatientId,
                PatientFullName = lr.LabRequestItem.LabRequest.Examination.Appointment!.Patient!.FirstName + " " + lr.LabRequestItem.LabRequest.Examination.Appointment!.Patient!.LastName,
                DoctorId = lr.LabRequestItem.LabRequest.Examination.Appointment!.DoctorId,
                DoctorFullName = lr.LabRequestItem.LabRequest.Examination.Appointment!.Doctor!.FirstName + " " + lr.LabRequestItem.LabRequest.Examination.Appointment!.Doctor!.LastName,
                TestName = lr.LabRequestItem.TestName,
                ResultValue = lr.ResultValue,
                Units = lr.Units,
                ReferenceRange = lr.ReferenceRange,
                Notes = lr.Notes,
                ResultDate = lr.ResultDate
            })
            .ToListAsync(cancellationToken);
    }
}
