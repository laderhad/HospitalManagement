namespace HospitalManagement.Application.Doctors.Queries.GetDoctorById;

public sealed record DoctorDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    Guid DepartmentId,
    string DepartmentName,
    string ContactNumber,
    string ApplicationUserId,
    string? Specialty,
    string? Title,
    bool IsActive);