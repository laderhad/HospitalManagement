using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Examination : BaseAuditableEntity
{
    private readonly List<Prescription> _prescriptions = new();
    private readonly List<LabRequest> _labRequests = new();

    private Examination() { }

    public Examination(Guid appointmentId, string diagnosis, string treatment)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));
        }

        if (string.IsNullOrWhiteSpace(diagnosis))
        {
            throw new ArgumentException("Diagnosis is required.", nameof(diagnosis));
        }

        if (string.IsNullOrWhiteSpace(treatment))
        {
            throw new ArgumentException("Treatment is required.", nameof(treatment));
        }

        AppointmentId = appointmentId;
        Diagnosis = diagnosis;
        Treatment = treatment;
    }

    public Guid AppointmentId { get; private set; }
    public Appointment? Appointment { get; set; }
    public string Diagnosis { get; private set; } = string.Empty;
    public string Treatment { get; private set; } = string.Empty;
    public IReadOnlyCollection<Prescription> Prescriptions => _prescriptions.AsReadOnly();
    public IReadOnlyCollection<LabRequest> LabRequests => _labRequests.AsReadOnly();

    public void UpdateAssessment(string diagnosis, string treatment)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
        {
            throw new ArgumentException("Diagnosis is required.", nameof(diagnosis));
        }

        if (string.IsNullOrWhiteSpace(treatment))
        {
            throw new ArgumentException("Treatment is required.", nameof(treatment));
        }

        Diagnosis = diagnosis;
        Treatment = treatment;
    }

    public Prescription AddPrescription(string medicationName, string dosage, string frequency, int durationDays, string? notes = null)
    {
        var prescription = new Prescription(this, medicationName, dosage, frequency, durationDays, notes);
        _prescriptions.Add(prescription);
        return prescription;
    }
}
