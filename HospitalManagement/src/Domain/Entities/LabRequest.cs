using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class LabRequest : BaseAuditableEntity
{
    private readonly List<LabRequestItem> _items = [];

    private LabRequest() { }

    public LabRequest(Guid examinationId, IEnumerable<string> testNames)
    {
        if (examinationId == Guid.Empty)
        {
            throw new DomainException("Examination id is required.", nameof(examinationId));
        }

        ExaminationId = examinationId;
        RequestDate = DateTime.UtcNow;
        AddItems(testNames);
    }

    internal LabRequest(Examination examination, IEnumerable<string> testNames)
    {
        Examination = examination ?? throw new DomainException("Examination is required.", nameof(examination));
        ExaminationId = examination.Id;
        RequestDate = DateTime.UtcNow;
        AddItems(testNames);
    }

    public Guid ExaminationId { get; private set; }
    public Examination? Examination { get; set; }
    public DateTime RequestDate { get; private set; }
    public IReadOnlyCollection<LabRequestItem> Items => _items.AsReadOnly();

    public LabRequestItem AddItem(string testName)
    {
        var item = new LabRequestItem(this, testName);
        _items.Add(item);
        return item;
    }

    private void AddItems(IEnumerable<string> testNames)
    {
        if (testNames is null)
        {
            throw new DomainException("Lab test names are required.", nameof(testNames));
        }

        foreach (var testName in testNames)
        {
            AddItem(testName);
        }

        if (_items.Count == 0)
        {
            throw new DomainException("At least one lab test is required.", nameof(testNames));
        }
    }
}
