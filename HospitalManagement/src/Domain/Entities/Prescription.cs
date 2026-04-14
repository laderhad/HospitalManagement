using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Prescription : BaseAuditableEntity
{
    private Prescription() { }

    public Prescription(Guid examinationId, string medicationName, string dosage, string frequency, int durationDays, string? notes = null)
    {
        if (examinationId == Guid.Empty)
        {
            throw new ArgumentException("Examination id is required.", nameof(examinationId));
        }

        SetDetails(medicationName, dosage, frequency, durationDays, notes);
        ExaminationId = examinationId;
    }

    internal Prescription(Examination examination, string medicationName, string dosage, string frequency, int durationDays, string? notes = null)
    {
        Examination = examination ?? throw new ArgumentNullException(nameof(examination));
        ExaminationId = examination.Id;
        SetDetails(medicationName, dosage, frequency, durationDays, notes);
    }

    public Guid ExaminationId { get; private set; }
    public Examination? Examination { get; set; }
    public string MedicationName { get; private set; } = string.Empty;
    public string Dosage { get; private set; } = string.Empty;
    public string Frequency { get; private set; } = string.Empty;
    public int DurationDays { get; private set; }
    public string? Notes { get; private set; }

    private void SetDetails(string medicationName, string dosage, string frequency, int durationDays, string? notes)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
        {
            throw new ArgumentException("Medication name is required.", nameof(medicationName));
        }

        if (string.IsNullOrWhiteSpace(dosage))
        {
            throw new ArgumentException("Dosage is required.", nameof(dosage));
        }

        if (string.IsNullOrWhiteSpace(frequency))
        {
            throw new ArgumentException("Frequency is required.", nameof(frequency));
        }

        if (durationDays <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(durationDays));
        }

        MedicationName = medicationName;
        Dosage = dosage;
        Frequency = frequency;
        DurationDays = durationDays;
        Notes = notes;
    }
}
