namespace HospitalManagement.Domain.Exceptions;

public class DomainOperationException : InvalidOperationException
{
    public DomainOperationException(string message)
        : base(message)
    {
    }
}
