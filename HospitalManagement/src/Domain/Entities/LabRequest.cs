using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class LabRequest : BaseAuditableEntity
{
    public Guid ExaminationId { get; set; }
    public Examination? Examination { get; set; }
    public DateTime RequestDate { get; set; }
    public ICollection<LabRequestItem> Items { get; set; } = new List<LabRequestItem>();
}
