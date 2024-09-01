namespace OrderConsumer.Service.Exceptions;

public class InvalidIdException : Exception
{
    public InvalidIdException(string id)
        : base($"Id: {id} is invalid and cannot be casted to Guid")
    {
    }
}
