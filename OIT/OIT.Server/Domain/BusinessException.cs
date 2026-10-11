namespace OIT.Server.Domain;

public class BusinessException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}
