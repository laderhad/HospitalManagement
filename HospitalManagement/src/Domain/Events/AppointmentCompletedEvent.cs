namespace HospitalManagement.Domain.Events;

public class AppointmentCompletedEvent : BaseEvent
{
    public AppointmentCompletedEvent(Appointment appointment)
    {
        Appointment = appointment;
    }

    public Appointment Appointment { get; }
}
