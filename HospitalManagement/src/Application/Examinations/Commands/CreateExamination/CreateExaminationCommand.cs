using FluentValidation.Results;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.Examinations.Commands.CreateExamination;

[Authorize]
public record CreateExaminationCommand : IRequest<Guid>
{
    public Guid AppointmentId { get; init; }

    public string Diagnosis { get; init; } = string.Empty;

    public string Treatment { get; init; } = string.Empty;
}

public class CreateExaminationCommandHandler : IRequestHandler<CreateExaminationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateExaminationCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreateExaminationCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Id == request.AppointmentId)
            .Select(a => new { a.Id, a.PatientId, a.DoctorId, a.Status })
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.AppointmentId, appointment);

        await EnsureCanManageAsync(appointment.DoctorId, cancellationToken);

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            throw ToValidationException(nameof(request.AppointmentId), ["Cancelled appointments cannot have examinations."]);
        }

        if (await _context.Examinations.AnyAsync(e => e.AppointmentId == request.AppointmentId, cancellationToken))
        {
            throw ToValidationException(nameof(request.AppointmentId), ["An examination already exists for this appointment."]);
        }

        var examination = new Examination(
            request.AppointmentId,
            request.Diagnosis.Trim(),
            request.Treatment.Trim());

        _context.Examinations.Add(examination);

        await _context.SaveChangesAsync(cancellationToken);

        return examination.Id;
    }

    private async Task EnsureCanManageAsync(Guid doctorId, CancellationToken cancellationToken)
    {
        if (ClinicalAccessHelper.IsAdministrator(_user))
        {
            return;
        }

        if (ClinicalAccessHelper.IsDoctor(_user))
        {
            var currentDoctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

            if (currentDoctorId == doctorId)
            {
                return;
            }
        }

        throw new ForbiddenAccessException();
    }

    private static HospitalManagement.Application.Common.Exceptions.ValidationException ToValidationException(string propertyName, IEnumerable<string> errors)
    {
        return new HospitalManagement.Application.Common.Exceptions.ValidationException(errors.Select(error => new ValidationFailure(propertyName, error)));
    }
}
