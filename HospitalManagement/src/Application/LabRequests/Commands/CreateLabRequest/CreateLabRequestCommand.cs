using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.LabRequests.Commands.CreateLabRequest;

[Authorize]
public record CreateLabRequestCommand : IRequest<Guid>
{
    public Guid ExaminationId { get; init; }

    public IReadOnlyCollection<string> TestNames { get; init; } = [];
}

public class CreateLabRequestCommandHandler : IRequestHandler<CreateLabRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateLabRequestCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Guid> Handle(CreateLabRequestCommand request, CancellationToken cancellationToken)
    {
        var examination = await _context.Examinations
            .Include(e => e.Appointment)
            .Include(e => e.LabRequests)
            .SingleOrDefaultAsync(e => e.Id == request.ExaminationId, cancellationToken);

        Guard.Against.NotFound(request.ExaminationId, examination);

        await EnsureCanManageAsync(examination.Appointment!.DoctorId, cancellationToken);

        var testNames = request.TestNames
            .Select(testName => testName.Trim())
            .Where(testName => !string.IsNullOrWhiteSpace(testName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var labRequest = examination.AddLabRequest(testNames);

        await _context.SaveChangesAsync(cancellationToken);

        return labRequest.Id;
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
