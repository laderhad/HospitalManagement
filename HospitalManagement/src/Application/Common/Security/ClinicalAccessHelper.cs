using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Common.Security;

internal static class ClinicalAccessHelper
{
    public static bool IsAdministrator(IUser user) => user.Roles?.Contains(Roles.Administrator) == true;

    public static bool IsDoctor(IUser user) => user.Roles?.Contains(Roles.Doctor) == true;

    public static bool IsPatient(IUser user) => user.Roles?.Contains(Roles.Patient) == true;

    public static async Task<Guid> GetRequiredDoctorProfileIdAsync(IApplicationDbContext context, IUser user, CancellationToken cancellationToken)
    {
        var userId = Guard.Against.Null(user.Id);

        var doctorId = await context.Doctors
            .AsNoTracking()
            .Where(d => d.ApplicationUserId == userId)
            .Select(d => (Guid?)d.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (doctorId is null)
        {
            throw new ForbiddenAccessException();
        }

        return doctorId.Value;
    }

    public static async Task<Guid> GetRequiredPatientProfileIdAsync(IApplicationDbContext context, IUser user, CancellationToken cancellationToken)
    {
        var userId = Guard.Against.Null(user.Id);

        var patientId = await context.Patients
            .AsNoTracking()
            .Where(p => p.ApplicationUserId == userId)
            .Select(p => (Guid?)p.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (patientId is null)
        {
            throw new ForbiddenAccessException();
        }

        return patientId.Value;
    }
}
