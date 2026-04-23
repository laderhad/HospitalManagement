namespace HospitalManagement.Domain.Events;

public class AppointmentRescheduledEvent : BaseEvent
{
    public AppointmentRescheduledEvent(Appointment appointment)
    {
        Appointment = appointment;
    }

    public Appointment Appointment { get; }
}
