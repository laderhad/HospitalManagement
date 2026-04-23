using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Appointment : BaseAuditableEntity
{
    private Appointment() { }

    public Appointment(Guid patientId, Guid doctorId, DateTime appointmentDate)
    {
        if (patientId == Guid.Empty)
        {
            throw new DomainException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new DomainException("Doctor id is required.", nameof(doctorId));
        }

        if (appointmentDate <= DateTime.UtcNow)
        {
            throw new DomainException("Appointment date must be in the future.", nameof(appointmentDate));
        }

        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentDate = appointmentDate;
        Status = AppointmentStatus.Scheduled;
        AddDomainEvent(new AppointmentScheduledEvent(this));
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
            throw new DomainOperationException("Cancelled appointments cannot be rescheduled.");
        }

        if (Status == AppointmentStatus.Completed)
        {
            throw new DomainOperationException("Completed appointments cannot be rescheduled.");
        }

        if (appointmentDate <= DateTime.UtcNow)
        {
            throw new DomainException("Appointment date must be in the future.", nameof(appointmentDate));
        }

        AppointmentDate = appointmentDate;
        AddDomainEvent(new AppointmentRescheduledEvent(this));
    }

    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
        {
            throw new DomainOperationException("Cancelled appointments cannot be completed.");
        }

        Status = AppointmentStatus.Completed;
        AddDomainEvent(new AppointmentCompletedEvent(this));
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
        {
            throw new DomainOperationException("Completed appointments cannot be cancelled.");
        }

        Status = AppointmentStatus.Cancelled;
        AddDomainEvent(new AppointmentCancelledEvent(this));
    }
}
