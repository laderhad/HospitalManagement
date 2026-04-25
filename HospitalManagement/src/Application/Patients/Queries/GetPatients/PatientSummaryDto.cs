using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Patients.Queries.GetPatients;

public sealed record PatientSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    string ApplicationUserId);