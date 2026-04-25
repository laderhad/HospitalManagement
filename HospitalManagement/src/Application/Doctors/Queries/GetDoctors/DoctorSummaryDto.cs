namespace HospitalManagement.Application.Doctors.Queries.GetDoctors;

public sealed record DoctorSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    Guid DepartmentId,
    string DepartmentName,
    bool IsActive);