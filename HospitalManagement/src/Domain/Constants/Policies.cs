namespace HospitalManagement.Domain.Constants;

public abstract class Policies
{
    public const string IsAdministrator = nameof(IsAdministrator);
    public const string IsDoctor = nameof(IsDoctor);
    public const string IsPatient = nameof(IsPatient);
    public const string IsAdministratorOrDoctor = nameof(IsAdministratorOrDoctor);
    public const string IsAdministratorOrPatient = nameof(IsAdministratorOrPatient);
}
