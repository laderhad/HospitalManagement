using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Patients.Queries.GetMyPatientProfile;

[Authorize(Roles = Roles.Patient)]
public record GetMyPatientProfileQuery : IRequest<MyPatientProfileDto>;

public class GetMyPatientProfileQueryHandler : IRequestHandler<GetMyPatientProfileQuery, MyPatientProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyPatientProfileQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<MyPatientProfileDto> Handle(GetMyPatientProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = Guard.Against.Null(_user.Id);

        var patient = await _context.Patients
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);

        Guard.Against.NotFound(userId, patient);

        return new MyPatientProfileDto(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.DateOfBirth,
            patient.Gender,
            patient.ContactNumber.Value,
            patient.Address.Value);
    }
}