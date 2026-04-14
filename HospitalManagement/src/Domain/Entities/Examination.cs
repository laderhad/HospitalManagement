using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Examination : BaseAuditableEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public required string Diagnosis { get; set; }
    public required string Treatment { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<LabRequest> LabRequests { get; set; } = new List<LabRequest>();
}
