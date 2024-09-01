using FluentAssertions;
using Moq;
using OrderConsumer.Service.Domain;
using OrderConsumer.Service.Exceptions;
using OrderConsumer.Service.Services;
using OrderProducer.Service.Messages;

namespace OrderConsumer.Tests;

public class OrderTests
{
    [Fact]
    public void CreateOrderFromNewOrderMessage_WhenNewOrderMessageIsNull_ThenThrowArgumentNullException()
    {
        //Arrange
        NewOrderMessage message = null!;
        var timeProvider = new TimeProvider();

        //Act
        Action action = () => Order.CreateOrderFromNewOrderMessage(message!, timeProvider);

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalidId")]
    [InlineData("a41d3c31-1d84-4b3b-ba11-558f2a2048e5aaaa")]
    public void CreateOrderFromNewOrderMessage_WhenIdIsInvalid_ThenThrowInvalidIdException(string id)
    {
        //Arrange
        var message = new NewOrderMessage
        {
            Id = id
        };

        var timeProvider = new TimeProvider();

        //Act
        Action action = () => Order.CreateOrderFromNewOrderMessage(message, timeProvider);

        //Assert
        action.Should().Throw<InvalidIdException>();
    }

    [Fact]
    public void CreateOrderFromNewOrderMessage_WhenQuantityIsLessThanOne_ThenThrowInvalidOrderQuantityException()
    {
        //Arrange
        var message = new NewOrderMessage
        {
            Id = Guid.NewGuid().ToString(),
            Quantity = -1
        };

        var timeProvider = new TimeProvider();

        //Act
        Action action = () => Order.CreateOrderFromNewOrderMessage(message, timeProvider);

        //Assert
        action.Should().Throw<InvalidOrderQuantityException>();
    }

    [Fact]
    public void CreateOrderFromNewOrderMessage_WhenPriceIsLessThanZero_ThenThrowInvalidOrderPriceException()
    {
        //Arrange
        var message = new NewOrderMessage
        {
            Id = Guid.NewGuid().ToString(),
            Quantity = 1,
            Price = -1
        };

        var timeProvider = new TimeProvider();

        //Act
        Action action = () => Order.CreateOrderFromNewOrderMessage(message, timeProvider);

        //Assert
        action.Should().Throw<InvalidOrderPriceException>();
    }

    [Fact]
    public void CreateOrderFromNewOrderMessage_WhenCreatedAtIsAfterUtcNow_ThenThrowInvalidOrderCreatedAtDateException()
    {
        //Arrange
        var message = new NewOrderMessage
        {
            Id = Guid.NewGuid().ToString(),
            Quantity = 1,
            Price = 2,
            CreatedAt = DateTime.UtcNow.AddDays(1)
        };

        var timeProvider = new Mock<ITimeProvider>();
        timeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        //Act
        Action action = () => Order.CreateOrderFromNewOrderMessage(message, timeProvider.Object);

        //Assert
        action.Should().Throw<InvalidOrderCreatedAtDateException>();
    }

    [Fact]
    public void CreateOrderFromNewOrderMessage_WhenMessageIsValid_ThenReturnsNewOrder()
    {
        //`
        //Arrange
        var message = new NewOrderMessage
        {
            Id = Guid.NewGuid().ToString(),
            Quantity = 12,
            Price = 14.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var timeProvider = new Mock<ITimeProvider>();
        var fixedTime = DateTime.UtcNow;
        timeProvider.Setup(x => x.UtcNow).Returns(fixedTime);

        //Act
        var order = Order.CreateOrderFromNewOrderMessage(message, timeProvider.Object);

        //Assert
        order.Id.Should().Be(message.Id);
        order.Quantity.Should().Be(message.Quantity);
        order.Price.Should().Be(message.Price);
        order.OrderCreatedAt.Should().Be(message.CreatedAt);
        order.CreatedAt.Should().Be(fixedTime);
        order.TotalPrice.Should().Be(message.Quantity * message.Price);
    }
}