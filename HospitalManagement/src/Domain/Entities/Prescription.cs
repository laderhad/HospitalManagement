using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Prescription : BaseAuditableEntity
{
    public Guid ExaminationId { get; set; }
    public Examination? Examination { get; set; }
    public required string MedicationName { get; set; }
    public required string Dosage { get; set; }
    public required string Frequency { get; set; }
    public int DurationDays { get; set; }
    public string? Notes { get; set; }
}
