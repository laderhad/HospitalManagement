namespace HospitalManagement.Domain.Events;

public class LabResultAddedEvent : BaseEvent
{
    public LabResultAddedEvent(LabResult labResult)
    {
        LabResult = labResult;
    }

    public LabResult LabResult { get; }
}
