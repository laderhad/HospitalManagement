using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Appointment : BaseAuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DateTime AppointmentDate { get; set; }
    public required string Status { get; set; } // e.g., "Scheduled", "Completed", "Cancelled"
}
