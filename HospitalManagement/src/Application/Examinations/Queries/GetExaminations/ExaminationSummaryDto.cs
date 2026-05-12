namespace HospitalManagement.Application.Examinations.Queries.GetExaminations;

public class ExaminationSummaryDto
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

    public int PrescriptionCount { get; init; }

    public int LabRequestCount { get; init; }
}
