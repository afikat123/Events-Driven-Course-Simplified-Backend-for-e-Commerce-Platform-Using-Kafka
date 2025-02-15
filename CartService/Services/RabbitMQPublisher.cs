using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CartService.Services;

public class RabbitMQPublisher
{
    private readonly IConfiguration _config;

    public RabbitMQPublisher(IConfiguration config)
    {
        _config = config;
    }

    public void PublishOrder(Order order)
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest"
        };


        //using var connection = factory.CreateConnection();
        try
        {
            Console.WriteLine("Attempting to connect to RabbitMQ...");
            var connection = factory.CreateConnection();
            Console.WriteLine("Connected to RabbitMQ!");
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("orderExchange", ExchangeType.Fanout);

            var message = JsonConvert.SerializeObject(order);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("orderExchange", "", null, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RabbitMQ connection failed: {ex.Message}");
            throw;
        }
      
    }
}
