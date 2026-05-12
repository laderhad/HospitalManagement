using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Appointments.Queries.GetAppointments;

public sealed record AppointmentSummaryDto(
    Guid Id,
    DateTime AppointmentDate,
    AppointmentStatus Status,
    Guid PatientId,
    string PatientFullName,
    Guid DoctorId,
    string DoctorFullName);
