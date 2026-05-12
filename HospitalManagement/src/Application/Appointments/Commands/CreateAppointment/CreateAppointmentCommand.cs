using FluentValidation.Results;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Appointments.Commands.CreateAppointment;

[Authorize(Roles = Roles.Administrator)]
public record CreateAppointmentCommand : IRequest<Guid>
{
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime AppointmentDate { get; init; }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointmentDate = NormalizeToUtc(request.AppointmentDate);

        await EnsureNoConflictAsync(request.PatientId, request.DoctorId, appointmentDate, null, cancellationToken);

        var entity = new Appointment(request.PatientId, request.DoctorId, appointmentDate);
        _context.Appointments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    internal async Task EnsureNoConflictAsync(Guid patientId, Guid doctorId, DateTime appointmentDate, Guid? ignoredAppointmentId, CancellationToken cancellationToken)
    {
        var hasDoctorConflict = await _context.Appointments.AnyAsync(
            a => a.DoctorId == doctorId
                && a.AppointmentDate == appointmentDate
                && a.Status != AppointmentStatus.Cancelled
                && (!ignoredAppointmentId.HasValue || a.Id != ignoredAppointmentId.Value),
            cancellationToken);

        if (hasDoctorConflict)
        {
            throw ToValidationException(nameof(CreateAppointmentCommand.DoctorId), ["The doctor already has an appointment at the selected time."]);
        }

        var hasPatientConflict = await _context.Appointments.AnyAsync(
            a => a.PatientId == patientId
                && a.AppointmentDate == appointmentDate
                && a.Status != AppointmentStatus.Cancelled
                && (!ignoredAppointmentId.HasValue || a.Id != ignoredAppointmentId.Value),
            cancellationToken);

        if (hasPatientConflict)
        {
            throw ToValidationException(nameof(CreateAppointmentCommand.PatientId), ["The patient already has an appointment at the selected time."]);
        }
    }

    internal static DateTime NormalizeToUtc(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }

    internal static HospitalManagement.Application.Common.Exceptions.ValidationException ToValidationException(string propertyName, IEnumerable<string> errors)
    {
        return new HospitalManagement.Application.Common.Exceptions.ValidationException(errors.Select(error => new ValidationFailure(propertyName, error)));
    }
}
