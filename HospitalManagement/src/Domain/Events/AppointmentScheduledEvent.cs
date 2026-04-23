namespace HospitalManagement.Domain.Events;

public class AppointmentScheduledEvent : BaseEvent
{
    public AppointmentScheduledEvent(Appointment appointment)
    {
        Appointment = appointment;
    }

    public Appointment Appointment { get; }
}
