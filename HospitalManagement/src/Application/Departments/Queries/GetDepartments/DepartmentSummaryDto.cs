namespace HospitalManagement.Application.Departments.Queries.GetDepartments;

public sealed record DepartmentSummaryDto(Guid Id, string Name, bool IsActive);