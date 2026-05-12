using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Appointments.Commands.CreateMyAppointment;

[Authorize(Roles = Roles.Patient)]
public record CreateMyAppointmentCommand : IRequest<Guid>
{
    public Guid DoctorId { get; init; }
    public DateTime AppointmentDate { get; init; }
}

public class CreateMyAppointmentCommandHandler : IRequestHandler<CreateMyAppointmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateMyAppointmentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreateMyAppointmentCommand request, CancellationToken cancellationToken)
    {
        var patientId = await ClinicalAccessHelper.GetRequiredPatientProfileIdAsync(_context, _user, cancellationToken);
        var appointmentDate = CreateAppointmentCommandHandler.NormalizeToUtc(request.AppointmentDate);

        await EnsureNoConflictAsync(patientId, request.DoctorId, appointmentDate, cancellationToken);

        var entity = new Appointment(patientId, request.DoctorId, appointmentDate);
        _context.Appointments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private async Task EnsureNoConflictAsync(Guid patientId, Guid doctorId, DateTime appointmentDate, CancellationToken cancellationToken)
    {
        var hasDoctorConflict = await _context.Appointments.AnyAsync(
            a => a.DoctorId == doctorId && a.AppointmentDate == appointmentDate && a.Status != Domain.Enums.AppointmentStatus.Cancelled,
            cancellationToken);

        if (hasDoctorConflict)
        {
            throw CreateAppointmentCommandHandler.ToValidationException(nameof(CreateMyAppointmentCommand.DoctorId), ["The doctor already has an appointment at the selected time."]);
        }

        var hasPatientConflict = await _context.Appointments.AnyAsync(
            a => a.PatientId == patientId && a.AppointmentDate == appointmentDate && a.Status != Domain.Enums.AppointmentStatus.Cancelled,
            cancellationToken);

        if (hasPatientConflict)
        {
            throw CreateAppointmentCommandHandler.ToValidationException(nameof(CreateMyAppointmentCommand.AppointmentDate), ["You already have an appointment at the selected time."]);
        }
    }
}
