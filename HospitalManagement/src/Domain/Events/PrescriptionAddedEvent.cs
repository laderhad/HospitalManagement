namespace HospitalManagement.Domain.Events;

public class PrescriptionAddedEvent : BaseEvent
{
    public PrescriptionAddedEvent(Prescription prescription)
    {
        Prescription = prescription;
    }

    public Prescription Prescription { get; }
}
