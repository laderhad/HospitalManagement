using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Examination : BaseAuditableEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
}
