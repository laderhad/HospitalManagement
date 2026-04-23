namespace HospitalManagement.Domain.Events;

public class ExaminationCreatedEvent : BaseEvent
{
    public ExaminationCreatedEvent(Examination examination)
    {
        Examination = examination;
    }

    public Examination Examination { get; }
}
