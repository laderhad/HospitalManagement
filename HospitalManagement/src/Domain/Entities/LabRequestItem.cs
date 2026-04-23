using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class LabRequestItem : BaseAuditableEntity
{
    private readonly List<LabResult> _results = [];

    private LabRequestItem() { }

    public LabRequestItem(Guid labRequestId, string testName)
    {
        if (labRequestId == Guid.Empty)
        {
            throw new DomainException("Lab request id is required.", nameof(labRequestId));
        }

        LabRequestId = labRequestId;
        SetTestName(testName);
    }

    internal LabRequestItem(LabRequest labRequest, string testName)
    {
        LabRequest = labRequest ?? throw new DomainException("Lab request is required.", nameof(labRequest));
        LabRequestId = labRequest.Id;
        SetTestName(testName);
    }

    public Guid LabRequestId { get; private set; }
    public LabRequest? LabRequest { get; set; }
    public string TestName { get; private set; } = string.Empty;
    public IReadOnlyCollection<LabResult> Results => _results.AsReadOnly();

    public LabResult AddResult(string resultValue, string units, string referenceRange, string? notes = null)
    {
        var result = new LabResult(this, resultValue, units, referenceRange, notes);
        _results.Add(result);
        AddDomainEvent(new LabResultAddedEvent(result));
        return result;
    }

    private void SetTestName(string testName)
    {
        if (string.IsNullOrWhiteSpace(testName))
        {
            throw new DomainException("Test name is required.", nameof(testName));
        }

        TestName = testName;
    }
}
