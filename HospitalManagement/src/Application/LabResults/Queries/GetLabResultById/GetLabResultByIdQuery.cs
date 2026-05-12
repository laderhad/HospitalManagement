using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.LabResults.Queries.GetLabResultById;

[Authorize]
public record GetLabResultByIdQuery(Guid Id) : IRequest<LabResultDetailsDto>;

public class GetLabResultByIdQueryHandler : IRequestHandler<GetLabResultByIdQuery, LabResultDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetLabResultByIdQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<LabResultDetailsDto> Handle(GetLabResultByIdQuery request, CancellationToken cancellationToken)
    {
        var labResult = await _context.LabResults
            .AsNoTracking()
            .Where(lr => lr.Id == request.Id)
            .Select(lr => new
            {
                Details = new LabResultDetailsDto
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
                },
                PatientId = lr.LabRequestItem.LabRequest.Examination!.Appointment!.PatientId,
                DoctorId = lr.LabRequestItem.LabRequest.Examination.Appointment!.DoctorId
            })
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, labResult);

        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return labResult.Details;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (doctorId == labResult.DoctorId)
            {
                return labResult.Details;
            }
        }

        if (ClinicalAccessHelper.IsPatient(_user))
        {
            var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

            if (patientId == labResult.PatientId)
            {
                return labResult.Details;
            }
        }

        throw new ForbiddenAccessException();
    }
}
