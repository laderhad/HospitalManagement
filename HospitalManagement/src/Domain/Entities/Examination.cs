using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Examination : BaseAuditableEntity
{
    private readonly List<Prescription> _prescriptions = [];
    private readonly List<LabRequest> _labRequests = [];

    private Examination() { }

    public Examination(Guid appointmentId, string diagnosis, string treatment)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new DomainException("Appointment id is required.", nameof(appointmentId));
        }

        AppointmentId = appointmentId;
        UpdateAssessment(diagnosis, treatment);
        AddDomainEvent(new ExaminationCreatedEvent(this));
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
            throw new DomainException("Diagnosis is required.", nameof(diagnosis));
        }

        if (string.IsNullOrWhiteSpace(treatment))
        {
            throw new DomainException("Treatment is required.", nameof(treatment));
        }

        Diagnosis = diagnosis;
        Treatment = treatment;
    }

    public Prescription AddPrescription(string medicationName, string dosage, string frequency, int durationDays, string? notes = null)
    {
        var prescription = new Prescription(this, medicationName, dosage, frequency, durationDays, notes);
        _prescriptions.Add(prescription);
        AddDomainEvent(new PrescriptionAddedEvent(prescription));
        return prescription;
    }

    public LabRequest AddLabRequest(params string[] testNames)
    {
        var labRequest = new LabRequest(this, testNames);
        _labRequests.Add(labRequest);
        AddDomainEvent(new LabRequestCreatedEvent(labRequest));
        return labRequest;
    }
}
