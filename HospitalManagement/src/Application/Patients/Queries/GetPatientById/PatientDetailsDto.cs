using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Patients.Queries.GetPatientById;

public sealed record PatientDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    string ContactNumber,
    string Address,
    string ApplicationUserId);