using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
namespace CartService.Services;

public class KafkaProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly List<string> _orders = new();
    public readonly string newOrdersTopicName = "new-orders-topic";
    public readonly string updateOrdersTopicName = "update-orders-topic";
    private readonly Logger Logger;

    public KafkaProducer()
    {
        var config = new ProducerConfig
        {
            // BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_ADVERTISED_LISTENER") ?? "localhost:9092",
            // BootstrapServers = "127.0.0.1:9092",
            // BootstrapServers = "0.0.0.0:9092",
            // BootstrapServers = "broker:9092",
            // BootstrapServers =  "broker:29092",
            BootstrapServers = "127.0.0.1:9092,localhost:29092,broker:29092,localhost:29092",
            Acks = Acks.Leader,
            // RetryBackoffMs = 1000,
            // MessageTimeoutMs = 90000, // Time to wait for a message to be acknowledged before timing out
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
    }

    public void produceTestMessageAndCreateTopic()
    {
        Order order = Order.Generate("healthcheck", 1);
        var jsonOrder = JsonConvert.SerializeObject(order);
        var messageOrder = new Message<string, string>
        {
            Key = "healthcheck",
            Value = jsonOrder
        };
        _producer.Produce(newOrdersTopicName, messageOrder, deliveryReport =>
        {
            Logger.Information($"Test Message For Create Order Produced: {deliveryReport.Message.Value}");
        });
        UpdateOrder update = new UpdateOrder("healthcheck", "done");
        var jsonUpdate = JsonConvert.SerializeObject(update);
        var messageUpdate = new Message<string, string>
        {
            Key = "healthcheck",
            Value = jsonUpdate
        };
        _producer.Produce(updateOrdersTopicName, messageUpdate, deliveryReport =>
        {
            Logger.Information($"Test Message For UPDATE Order Produced: {deliveryReport.Message.Value}");
        });
    }

    public async Task produceMessage(object messageObject, string topicName)
    {
        if (topicName == newOrdersTopicName)
        {
            Order? order = messageObject as Order;
            if (order != null)
            {
                produceNewOrder(order);
            }
        }
        else if (topicName == updateOrdersTopicName)
        {
            UpdateOrder? update = messageObject as UpdateOrder;
            if (update != null)
            {
                produceUpdateMessage(update);
            }
        }
        else
        {
            Logger.Error($"Topic name {topicName} Not Found!");
            throw new Exception($"Topic {topicName} Not Found!");
        }
    }

    private async void produceNewOrder(Order order)
    {
        var json = JsonConvert.SerializeObject(order);
        var message = new Message<string, string>
        {
            Key = order.OrderId,
            Value = json
        };
        try
        {
            var result = await _producer.ProduceAsync(newOrdersTopicName, message);
            Logger.Information($"Delivered ${result.Value} to {result.TopicPartitionOffset}");
            _orders.Add(order.OrderId);         
        }
        catch (ProduceException<Null, string> e)
        {
            Logger.Error($"Error in producing message! {e.Error.Reason}");
            throw new Exception($"Error in producing message! {e.Message}");
        }
    }

    private async void produceUpdateMessage(UpdateOrder update)
    {
        var json = JsonConvert.SerializeObject(update);
        var message = new Message<string, string>
        {
            Key = update.OrderId,
            Value = json
        };
        try
        {
            var result = await _producer.ProduceAsync(updateOrdersTopicName, message);
            Logger.Information($"Delivered ${result.Value} to {result.TopicPartitionOffset}");
        }
        catch (ProduceException<Null, string> e)
        {
            Logger.Error($"Error in producing message! {e.Error.Reason}");
            throw new Exception($"Error in producing message! {e.Message}");
        }
    }

    public bool isOrderExist(string orderId) => _orders.Contains(orderId);
}