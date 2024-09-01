namespace OrderConsumer.Service.Exceptions;

public class InvalidOrderCreatedAtDateException : Exception
{
    public InvalidOrderCreatedAtDateException(DateTime orderCreatedAt, DateTime utcNow)
        : base($"OrderCreatedAt {orderCreatedAt} should be before utc now: {utcNow}")
    {
    }
}
