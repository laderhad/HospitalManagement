namespace HospitalManagement.Domain.Events;

public class AppointmentCancelledEvent : BaseEvent
{
    public AppointmentCancelledEvent(Appointment appointment)
    {
        Appointment = appointment;
    }

    public Appointment Appointment { get; }
}
