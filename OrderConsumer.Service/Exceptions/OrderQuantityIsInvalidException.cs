namespace OrderConsumer.Service.Exceptions;

public class OrderQuantityIsInvalidException : Exception
{
    public OrderQuantityIsInvalidException(Guid id, int quantity) 
        : base($"Order with id: {id} have invalid quantity: {quantity}") 
    {
    }
}
