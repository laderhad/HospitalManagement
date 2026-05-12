using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Appointments.Commands.RescheduleAppointment;

[Authorize]
public record RescheduleAppointmentCommand : IRequest
{
    public Guid Id { get; init; }
    public DateTime AppointmentDate { get; init; }
}

public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public RescheduleAppointmentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Appointments.SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, entity);

        await EnsureCanManageAsync(entity, cancellationToken);

        var appointmentDate = CreateAppointmentCommandHandler.NormalizeToUtc(request.AppointmentDate);
        await EnsureNoConflictAsync(entity, appointmentDate, cancellationToken);

        entity.Reschedule(appointmentDate);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCanManageAsync(Appointment entity, CancellationToken cancellationToken)
    {
        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (entity.DoctorId == doctorId)
            {
                return;
            }
        }

        if (ClinicalAccessHelper.IsPatient(_user))
        {
            var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);

            if (entity.PatientId == patientId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException();
    }

    private async Task EnsureNoConflictAsync(Appointment entity, DateTime appointmentDate, CancellationToken cancellationToken)
    {
        var hasDoctorConflict = await _context.Appointments.AnyAsync(
            a => a.Id != entity.Id
                && a.DoctorId == entity.DoctorId
                && a.AppointmentDate == appointmentDate
                && a.Status != Domain.Enums.AppointmentStatus.Cancelled,
            cancellationToken);

        if (hasDoctorConflict)
        {
            throw CreateAppointmentCommandHandler.ToValidationException(nameof(RescheduleAppointmentCommand.AppointmentDate), ["The doctor already has an appointment at the selected time."]);
        }

        var hasPatientConflict = await _context.Appointments.AnyAsync(
            a => a.Id != entity.Id
                && a.PatientId == entity.PatientId
                && a.AppointmentDate == appointmentDate
                && a.Status != Domain.Enums.AppointmentStatus.Cancelled,
            cancellationToken);

        if (hasPatientConflict)
        {
            throw CreateAppointmentCommandHandler.ToValidationException(nameof(RescheduleAppointmentCommand.AppointmentDate), ["The patient already has an appointment at the selected time."]);
        }
    }
}
