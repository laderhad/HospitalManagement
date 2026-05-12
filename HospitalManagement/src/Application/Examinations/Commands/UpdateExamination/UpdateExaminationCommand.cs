using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.Examinations.Commands.UpdateExamination;

[Authorize]
public record UpdateExaminationCommand : IRequest
{
    public Guid Id { get; init; }

    public string Diagnosis { get; init; } = string.Empty;

    public string Treatment { get; init; } = string.Empty;
}

public class UpdateExaminationCommandHandler : IRequestHandler<UpdateExaminationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateExaminationCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateExaminationCommand request, CancellationToken cancellationToken)
    {
        var examination = await _context.Examinations
            .Include(e => e.Appointment)
            .SingleOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, examination);

        await EnsureCanManageAsync(examination.Appointment!.DoctorId, cancellationToken);

        examination.UpdateAssessment(request.Diagnosis.Trim(), request.Treatment.Trim());

        await _context.SaveChangesAsync(cancellationToken);
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
}
