using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Doctors.Commands.SetDoctorActiveState;

[Authorize(Roles = Roles.Administrator)]
public record SetDoctorActiveStateCommand : IRequest
{
    public Guid Id { get; init; }

    public bool IsActive { get; init; }
}

public class SetDoctorActiveStateCommandHandler : IRequestHandler<SetDoctorActiveStateCommand>
{
    private readonly IApplicationDbContext _context;

    public SetDoctorActiveStateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SetDoctorActiveStateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Doctors
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        if (request.IsActive)
        {
            entity.Activate();
        }
        else
        {
            entity.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}