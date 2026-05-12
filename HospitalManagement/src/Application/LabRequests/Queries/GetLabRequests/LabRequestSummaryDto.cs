namespace HospitalManagement.Application.LabRequests.Queries.GetLabRequests;

public class LabRequestSummaryDto
{
    public Guid Id { get; init; }

    public Guid ExaminationId { get; init; }

    public Guid AppointmentId { get; init; }

    public Guid PatientId { get; init; }

    public string PatientFullName { get; init; } = string.Empty;

    public Guid DoctorId { get; init; }

    public string DoctorFullName { get; init; } = string.Empty;

    public DateTime RequestDate { get; init; }

    public int ItemCount { get; init; }

    public int ResultCount { get; init; }
}
