namespace HospitalManagement.Application.Departments.Queries.GetDepartmentById;

public sealed record DepartmentDetailsDto(Guid Id, string Name, bool IsActive);