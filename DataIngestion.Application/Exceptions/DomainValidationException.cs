namespace DataIngestion.Application.Exceptions
{
    public class DomainValidationException(string message) : Exception(message)
    {
    }
}
