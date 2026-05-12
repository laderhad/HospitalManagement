namespace HospitalManagement.Application.LabResults.Queries.GetLabResultById;

public class LabResultDetailsDto
{
    public Guid Id { get; init; }

    public Guid LabRequestId { get; init; }

    public Guid LabRequestItemId { get; init; }

    public Guid ExaminationId { get; init; }

    public Guid AppointmentId { get; init; }

    public Guid PatientId { get; init; }

    public string PatientFullName { get; init; } = string.Empty;

    public Guid DoctorId { get; init; }

    public string DoctorFullName { get; init; } = string.Empty;

    public string TestName { get; init; } = string.Empty;

    public string ResultValue { get; init; } = string.Empty;

    public string Units { get; init; } = string.Empty;

    public string ReferenceRange { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public DateTime ResultDate { get; init; }
}
