namespace OrderConsumer.Service.Database;

public interface IBaseContextRepository
{
    Task SaveChangesAsync();
}

public abstract class BaseContextRepository : IBaseContextRepository
{
    protected DataContext _context;

    protected BaseContextRepository(DataContext dataContext)
    {
        _context = dataContext;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
