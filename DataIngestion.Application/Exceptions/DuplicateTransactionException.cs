namespace DataIngestion.Application.Exceptions
{
    public class DuplicateTransactionException(string message) : Exception(message)
    {
    }
}
