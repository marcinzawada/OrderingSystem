namespace OrderProducer.Service.Messages;

public class NewOrderMessage
{
    public string? Id { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
