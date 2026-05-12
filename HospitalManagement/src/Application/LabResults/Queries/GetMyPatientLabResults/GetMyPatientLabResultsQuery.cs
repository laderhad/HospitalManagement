using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Application.LabResults.Queries.GetLabResults;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.LabResults.Queries.GetMyPatientLabResults;

[Authorize(Roles = Roles.Patient)]
public record GetMyPatientLabResultsQuery : IRequest<IReadOnlyCollection<LabResultSummaryDto>>;

public class GetMyPatientLabResultsQueryHandler : IRequestHandler<GetMyPatientLabResultsQuery, IReadOnlyCollection<LabResultSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyPatientLabResultsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IReadOnlyCollection<LabResultSummaryDto>> Handle(GetMyPatientLabResultsQuery request, CancellationToken cancellationToken)
    {
        var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

        return await _context.LabResults
            .AsNoTracking()
            .Where(lr => lr.LabRequestItem!.LabRequest!.Examination!.Appointment!.PatientId == patientId)
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
