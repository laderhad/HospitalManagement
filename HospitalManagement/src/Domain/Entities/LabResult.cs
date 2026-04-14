using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class LabResult : BaseAuditableEntity
{
    public Guid LabRequestItemId { get; set; }
    public LabRequestItem? LabRequestItem { get; set; }
    public required string ResultValue { get; set; }
    public required string Units { get; set; }
    public required string ReferenceRange { get; set; }
    public DateTime ResultDate { get; set; }
}
