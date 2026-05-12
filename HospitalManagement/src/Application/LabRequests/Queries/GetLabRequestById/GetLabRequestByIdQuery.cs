using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.LabRequests.Queries.GetLabRequestById;

[Authorize]
public record GetLabRequestByIdQuery(Guid Id) : IRequest<LabRequestDetailsDto>;

public class GetLabRequestByIdQueryHandler : IRequestHandler<GetLabRequestByIdQuery, LabRequestDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetLabRequestByIdQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<LabRequestDetailsDto> Handle(GetLabRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var labRequest = await _context.LabRequests
            .AsNoTracking()
            .Where(lr => lr.Id == request.Id)
            .Select(lr => new
            {
                Details = new LabRequestDetailsDto
                {
                    Id = lr.Id,
                    ExaminationId = lr.ExaminationId,
                    AppointmentId = lr.Examination!.AppointmentId,
                    PatientId = lr.Examination.Appointment!.PatientId,
                    PatientFullName = lr.Examination.Appointment.Patient!.FirstName + " " + lr.Examination.Appointment.Patient.LastName,
                    DoctorId = lr.Examination.Appointment.DoctorId,
                    DoctorFullName = lr.Examination.Appointment.Doctor!.FirstName + " " + lr.Examination.Appointment.Doctor.LastName,
                    RequestDate = lr.RequestDate,
                    Items = lr.Items
                        .OrderBy(item => item.TestName)
                        .Select(item => new LabRequestItemDto
                        {
                            Id = item.Id,
                            TestName = item.TestName,
                            ResultCount = item.Results.Count
                        })
                        .ToList()
                },
                PatientId = lr.Examination.Appointment!.PatientId,
                DoctorId = lr.Examination.Appointment.DoctorId
            })
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, labRequest);

        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return labRequest.Details;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (doctorId == labRequest.DoctorId)
            {
                return labRequest.Details;
            }
        }

        if (ClinicalAccessHelper.IsPatient(_user))
        {
            var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

            if (patientId == labRequest.PatientId)
            {
                return labRequest.Details;
            }
        }

        throw new ForbiddenAccessException();
    }
}
