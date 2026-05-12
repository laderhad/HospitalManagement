using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;

namespace HospitalManagement.Application.Appointments.Commands.CompleteAppointment;

[Authorize]
public record CompleteAppointmentCommand(Guid Id) : IRequest;

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CompleteAppointmentCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Appointments.SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, entity);

        if (!ClinicalAccessHelper.IsAdministrator(_user))
        {
            if (ClinicalAccessHelper.IsDoctor(_user))
            {
                var doctorId = await ClinicalAccessHelper.GetRequiredDoctorProfileIdAsync(_context, _user, cancellationToken);

                if (entity.DoctorId != doctorId)
                {
                    throw new ForbiddenAccessException();
                }
            }
            else
            {
                throw new ForbiddenAccessException();
            }
        }

        entity.Complete();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
