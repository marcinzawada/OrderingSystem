using OrderConsumer.Service.Exceptions;
using OrderConsumer.Service.Services;
using OrderProducer.Service.Messages;

namespace OrderConsumer.Service.Domain;

public class Order
{
    public Guid Id { get; private set; }

    public int Quantity { get; private set; }

    public decimal Price { get; private set; }

    public decimal TotalPrice { get; private set; }

    public DateTime OrderCreatedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Order()
    {
    }

    public static Order CreateOrderFromNewOrderMessage(NewOrderMessage message, ITimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var isIdValid = Guid.TryParse(message.Id, out Guid id);
        if (isIdValid is false)
        {
            throw new InvalidIdException(message.Id ?? string.Empty);
        }

        if (message.Quantity < 1)
        {
            throw new InvalidOrderQuantityException(id, message.Quantity);
        }

        if (message.Price < 0)
        {
            throw new InvalidOrderPriceException(id, message.Price);
        }

        if (message.CreatedAt > timeProvider.UtcNow)
        {
            throw new InvalidOrderCreatedAtDateException(message.CreatedAt, timeProvider.UtcNow);
        }

        var totalPrice = message.Quantity * message.Price;

        return new Order
        {
            Id = id,
            Quantity = message.Quantity,
            Price = message.Price,
            TotalPrice = totalPrice,
            OrderCreatedAt = message.CreatedAt,
            CreatedAt = timeProvider.UtcNow,
        };
    }
}
