using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Appointments.Queries.GetAppointmentById;

public sealed record AppointmentDetailsDto(
    Guid Id,
    DateTime AppointmentDate,
    AppointmentStatus Status,
    Guid PatientId,
    string PatientFullName,
    Guid DoctorId,
    string DoctorFullName,
    Guid DepartmentId,
    string DepartmentName);
