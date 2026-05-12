namespace HospitalManagement.Application.LabRequests.Queries.GetLabRequestById;

public class LabRequestDetailsDto
{
    public Guid Id { get; init; }

    public Guid ExaminationId { get; init; }

    public Guid AppointmentId { get; init; }

    public Guid PatientId { get; init; }

    public string PatientFullName { get; init; } = string.Empty;

    public Guid DoctorId { get; init; }

    public string DoctorFullName { get; init; } = string.Empty;

    public DateTime RequestDate { get; init; }

    public IReadOnlyCollection<LabRequestItemDto> Items { get; init; } = [];
}

public class LabRequestItemDto
{
    public Guid Id { get; init; }

    public string TestName { get; init; } = string.Empty;

    public int ResultCount { get; init; }
}
