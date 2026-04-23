using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class LabResult : BaseAuditableEntity
{
    private LabResult() { }

    public LabResult(Guid labRequestItemId, string resultValue, string units, string referenceRange, string? notes = null)
    {
        if (labRequestItemId == Guid.Empty)
        {
            throw new DomainException("Lab request item id is required.", nameof(labRequestItemId));
        }

        LabRequestItemId = labRequestItemId;
        SetDetails(resultValue, units, referenceRange, notes);
        ResultDate = DateTime.UtcNow;
    }

    internal LabResult(LabRequestItem labRequestItem, string resultValue, string units, string referenceRange, string? notes = null)
    {
        LabRequestItem = labRequestItem ?? throw new DomainException("Lab request item is required.", nameof(labRequestItem));
        LabRequestItemId = labRequestItem.Id;
        SetDetails(resultValue, units, referenceRange, notes);
        ResultDate = DateTime.UtcNow;
    }

    public Guid LabRequestItemId { get; private set; }
    public LabRequestItem? LabRequestItem { get; set; }
    public string ResultValue { get; private set; } = string.Empty;
    public string Units { get; private set; } = string.Empty;
    public string ReferenceRange { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime ResultDate { get; private set; }

    public void Correct(string resultValue, string units, string referenceRange, string? notes = null)
    {
        SetDetails(resultValue, units, referenceRange, notes);
        ResultDate = DateTime.UtcNow;
    }

    private void SetDetails(string resultValue, string units, string referenceRange, string? notes)
    {
        if (string.IsNullOrWhiteSpace(resultValue))
        {
            throw new DomainException("Result value is required.", nameof(resultValue));
        }

        if (string.IsNullOrWhiteSpace(units))
        {
            throw new DomainException("Units are required.", nameof(units));
        }

        if (string.IsNullOrWhiteSpace(referenceRange))
        {
            throw new DomainException("Reference range is required.", nameof(referenceRange));
        }

        ResultValue = resultValue;
        Units = units;
        ReferenceRange = referenceRange;
        Notes = notes;
    }
}
