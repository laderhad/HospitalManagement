using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Examination : BaseAuditableEntity
{
    private readonly List<Prescription> _prescriptions = [];
    private readonly List<LabRequest> _labRequests = [];

    public Guid AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public required string Diagnosis { get; set; }
    public required string Treatment { get; set; }

    public IReadOnlyCollection<Prescription> Prescriptions => _prescriptions.AsReadOnly();
    public IReadOnlyCollection<LabRequest> LabRequests => _labRequests.AsReadOnly();
}
