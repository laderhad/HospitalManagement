using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Appointment : BaseAuditableEntity
{
    private Appointment() { }

    public Appointment(Guid patientId, Guid doctorId, DateTime appointmentDate)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        if (appointmentDate <= DateTime.UtcNow)
        {
            throw new ArgumentException("Appointment date must be in the future.", nameof(appointmentDate));
        }

        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentDate = appointmentDate;
        Status = AppointmentStatus.Scheduled;
    }

    public Guid PatientId { get; private set; }
    public Patient? Patient { get; set; }
    public Guid DoctorId { get; private set; }
    public Doctor? Doctor { get; set; }
    public DateTime AppointmentDate { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;

    public void Reschedule(DateTime appointmentDate)
    {
        if (Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled appointments cannot be rescheduled.");
        }

        if (Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Completed appointments cannot be rescheduled.");
        }

        if (appointmentDate <= DateTime.UtcNow)
        {
            throw new ArgumentException("Appointment date must be in the future.", nameof(appointmentDate));
        }

        AppointmentDate = appointmentDate;
    }

    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled appointments cannot be completed.");
        }

        Status = AppointmentStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Completed appointments cannot be cancelled.");
        }

        Status = AppointmentStatus.Cancelled;
    }
}
