namespace HospitalManagement.Application.Prescriptions.Queries.GetPrescriptions;

public class PrescriptionSummaryDto
{
    public Guid Id { get; init; }

    public Guid ExaminationId { get; init; }

    public Guid AppointmentId { get; init; }

    public Guid PatientId { get; init; }

    public string PatientFullName { get; init; } = string.Empty;

    public Guid DoctorId { get; init; }

    public string DoctorFullName { get; init; } = string.Empty;

    public string MedicationName { get; init; } = string.Empty;

    public string Dosage { get; init; } = string.Empty;

    public string Frequency { get; init; } = string.Empty;

    public int DurationDays { get; init; }

    public string? Notes { get; init; }
}
