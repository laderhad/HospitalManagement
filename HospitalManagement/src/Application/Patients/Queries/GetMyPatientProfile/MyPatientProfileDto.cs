using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Patients.Queries.GetMyPatientProfile;

public sealed record MyPatientProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    string ContactNumber,
    string Address);