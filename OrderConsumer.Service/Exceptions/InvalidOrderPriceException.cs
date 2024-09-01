namespace OrderConsumer.Service.Exceptions;

public class InvalidOrderPriceException : Exception
{
    public InvalidOrderPriceException(Guid id, decimal price)
        : base($"Order with id: {id} have invalid price: {price}")
    {
    }
}
