using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Application.Common.Security;
using HospitalManagement.Domain.Constants;

namespace HospitalManagement.Application.Doctors.Queries.GetDoctorById;

[Authorize(Roles = Roles.Administrator)]
public record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDetailsDto>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, DoctorDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorDetailsDto> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, doctor);

        return new DoctorDetailsDto(
            doctor.Id,
            doctor.FirstName,
            doctor.LastName,
            doctor.DepartmentId,
            doctor.Department != null ? doctor.Department.Name : string.Empty,
            doctor.ContactNumber.Value,
            doctor.ApplicationUserId,
            doctor.Specialty,
            doctor.Title,
            doctor.IsActive);
    }
}