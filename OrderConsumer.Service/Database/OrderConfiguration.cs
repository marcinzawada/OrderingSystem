using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OrderConsumer.Service.Domain;

namespace OrderConsumer.Service.Database;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.OrderCreatedAt).IsRequired();

        builder.Property(t => t.Price).IsRequired();
        builder.Property(t => t.Quantity).IsRequired();
        builder.Property(t => t.TotalPrice).IsRequired();
    }
}
