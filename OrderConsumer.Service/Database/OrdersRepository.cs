using OrderConsumer.Service.Domain;

namespace OrderConsumer.Service.Database;

public interface IOrdersRepository : IBaseContextRepository
{
    Task AddAsync(Order order);
}

public class OrdersRepository : BaseContextRepository, IOrdersRepository
{
    public OrdersRepository(DataContext dataContext) 
        : base(dataContext) { }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }
}
