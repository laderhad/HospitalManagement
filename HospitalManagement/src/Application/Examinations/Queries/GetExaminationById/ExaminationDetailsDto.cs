namespace HospitalManagement.Application.Examinations.Queries.GetExaminationById;

public class ExaminationDetailsDto
{
    public Guid Id { get; init; }

    public Guid AppointmentId { get; init; }

    public DateTime AppointmentDate { get; init; }

    public Guid PatientId { get; init; }

    public string PatientFullName { get; init; } = string.Empty;

    public Guid DoctorId { get; init; }

    public string DoctorFullName { get; init; } = string.Empty;

    public string Diagnosis { get; init; } = string.Empty;

    public string Treatment { get; init; } = string.Empty;

    public IReadOnlyCollection<Guid> PrescriptionIds { get; init; } = [];

    public IReadOnlyCollection<Guid> LabRequestIds { get; init; } = [];
}
