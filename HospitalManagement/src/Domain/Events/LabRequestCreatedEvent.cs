namespace HospitalManagement.Domain.Events;

public class LabRequestCreatedEvent : BaseEvent
{
    public LabRequestCreatedEvent(LabRequest labRequest)
    {
        LabRequest = labRequest;
    }

    public LabRequest LabRequest { get; }
}
