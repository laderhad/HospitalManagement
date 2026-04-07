using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class LabRequestItem : BaseAuditableEntity
{
    public Guid LabRequestId { get; set; }
    public LabRequest LabRequest { get; set; }
    public string TestName { get; set; }
    public ICollection<LabResult> Results { get; set; } = new List<LabResult>();
}
