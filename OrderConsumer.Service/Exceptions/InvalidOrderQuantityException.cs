namespace OrderConsumer.Service.Exceptions;

public class InvalidOrderQuantityException : Exception
{
    public InvalidOrderQuantityException(Guid id, int quantity) 
        : base($"Order with id: {id} have invalid quantity: {quantity}") 
    {
    }
}
