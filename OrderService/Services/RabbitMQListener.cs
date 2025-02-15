using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using OrderService.Models;
using System.Threading;

namespace OrderService.Services;

public class RabbitMQListener
{
    private readonly IConfiguration _config;
    private readonly Dictionary<string, OrderDetails> _orders = new();

    public RabbitMQListener(IConfiguration config)
    {
        _config = config;
    }

    public void StartListening()
    {
        var factory = new ConnectionFactory
        {
            //HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq", // Ensure this matches the service name
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost", // Ensure this matches the service name
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest"
        };

        IConnection connection = null;
        IModel channel = null;
        const int maxRetries = 10;
        const int retryDelayMilliseconds = 8000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                Console.WriteLine("Successfully connected to RabbitMQ.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {attempt} to connect to RabbitMQ failed: {ex.Message}");

                if (attempt == maxRetries)
                {
                    Console.WriteLine("Max retry attempts reached. Unable to connect to RabbitMQ.");
                    throw;
                }

                Console.WriteLine($"Retrying in {retryDelayMilliseconds / 1000} seconds...");
                Thread.Sleep(retryDelayMilliseconds);
            }
        }

        if (channel == null)
        {
            throw new InvalidOperationException("Channel could not be created.");
        }

        channel.ExchangeDeclare(exchange: "orderExchange", type: ExchangeType.Fanout);

        const string queueName = "orderQueue";
        channel.QueueDeclare(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.QueueBind(queue: queueName, exchange: "orderExchange", routingKey: "");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, args) =>
        {
            var body = Encoding.UTF8.GetString(args.Body.ToArray());
            var order = JsonConvert.DeserializeObject<Order>(body);

            if (order != null && order.Status == "new")
            {
                var shippingCost = order.TotalAmount * 0.02;
                _orders[order.OrderId] = new OrderDetails
                {
                    OrderId = order.OrderId,
                    TotalAmount = order.TotalAmount,
                    ShippingCost = shippingCost,
                    Items = order.Items
                };

                Console.WriteLine($"Order processed: {order.OrderId}");
            }

            // Acknowledge the message
            channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        Console.WriteLine($"RabbitMQ Listener started on queue: {queueName}");
    }

    public OrderDetails? GetOrder(string orderId) => _orders.GetValueOrDefault(orderId);
}