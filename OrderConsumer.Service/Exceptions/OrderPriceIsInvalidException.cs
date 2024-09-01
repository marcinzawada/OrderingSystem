namespace OrderConsumer.Service.Exceptions;

public class OrderPriceIsInvalidException : Exception
{
    public OrderPriceIsInvalidException(Guid id, decimal price)
        : base($"Order with id: {id} have invalid price: {price}")
    {
    }
}
