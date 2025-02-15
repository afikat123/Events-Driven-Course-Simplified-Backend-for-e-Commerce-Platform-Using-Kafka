using System.Runtime.InteropServices;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Models;
namespace OrderService.Services;
using Serilog;
using Serilog.Core;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

public class KafkaConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly Dictionary<string, OrderDetails> _orders = new();
    private readonly Dictionary<string, HashSet<string>> _topicToOrdersDict = new();
    private readonly string newOrdersTopicName = "new-orders-topic";
    private readonly string updateOrdersTopicName = "update-orders-topic";
    private readonly Logger Logger;

    public KafkaConsumer()
    {
        var config = new ConsumerConfig
        {
            // BootstrapServers = "localhost:9092",
            // BootstrapServers = "127.0.0.1:9092",
            // BootstrapServers = "0.0.0.0:9092",
            // BootstrapServers =  "broker:29092",
            BootstrapServers = "broker:29092,127.0.0.1:9092,localhost:29092,localhost:9092",
            // GroupId = "group-test",
            GroupId = "group-test-" + Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
    }

    public void topicSubscribe()
    {
        if (_consumer != null)
        {
            _consumer.Subscribe(new List<string> { newOrdersTopicName, updateOrdersTopicName });
            Logger.Information($"Successfully Subscrbied to topics {newOrdersTopicName} and {updateOrdersTopicName}");
        }
        else
        {
            Logger.Error($"Consumer failed to subscribe");
        }
    }

    public async Task startConsume()
    {
        Thread.CurrentThread.Name = "ConsumeMessageThread";
        Logger.Debug($"[{Thread.CurrentThread.Name}]: In consumeMessages()");
        try
        {
            this.topicSubscribe();
            while (true)
            {
                Logger.Information($"[{Thread.CurrentThread.Name}]: Starting consume messages");
                var result = _consumer.Consume();
                if (result == null)
                {
                    Logger.Fatal($"[{Thread.CurrentThread.Name}]: Problem in consumer!!!");
                }
                else
                {
                    if (result.Topic.Equals(newOrdersTopicName))
                    {
                        HandleNewOrder(result);
                    }
                    else if (result.Topic.Equals(updateOrdersTopicName))
                    {
                        HandleUpdateOrder(result);
                    }
                    else
                    {
                        Logger.Error($"[{Thread.CurrentThread.Name}]: Unkown Topic");
                    }
                }

            }
        }
        catch (OperationCanceledException)
        {
            Logger.Error($"[{Thread.CurrentThread.Name}]: Consumer closed.");
        }
        catch (Exception e)
        {
            Logger.Error($"[{Thread.CurrentThread.Name}]: Error while consuming messages, Error: {e.Message}, \nStack Trace:\n {e.StackTrace}");
        }
        finally
        {
            _consumer.Close();
        }
    }

    public OrderDetails? GetOrder(string orderId) => _orders.GetValueOrDefault(orderId);

    public HashSet<string>? GetOrdersIdFromTopicName(string topicName) => _topicToOrdersDict.GetValueOrDefault(topicName);

    public void UpdateOrderStatus(string orderId, string newStatus)
    {
        var newOrder = _orders.GetValueOrDefault(orderId);
        if (newOrder != null)
        {
            newOrder.Status = newStatus;
            _orders.Remove(orderId);
            _orders.Add(orderId, newOrder);
        }
    }

    private void HandleNewOrder(ConsumeResult<string, string> result)
    {
        var orderJson = result.Message.Value;
        Logger.Information($"[{Thread.CurrentThread.Name}]: JSON Order {orderJson}");
        if (orderJson != null)
        {
            var order = JsonConvert.DeserializeObject<Order>(orderJson);
            var shippingCost = order.TotalAmount * 0.02;
            Logger.Information($"[{Thread.CurrentThread.Name}]: OrderID: {order.OrderId}");
            Logger.Information($"[{Thread.CurrentThread.Name}]: Order Status: {order.Status}");
            Logger.Information($"[{Thread.CurrentThread.Name}]: Total Amount: {order.TotalAmount}");
            Logger.Information($"[{Thread.CurrentThread.Name}]: Shipping Cost: {shippingCost}");
            Logger.Information($"[{Thread.CurrentThread.Name}]: Items: {order.Items}");
            _orders[order.OrderId] = new OrderDetails
            {
                OrderId = order.OrderId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingCost = shippingCost,
                Items = order.Items
            };
            Logger.Information($"[{Thread.CurrentThread.Name}]: Received Order: {order.OrderId} with Status: {order.Status} from topic {result.Topic}");
            if (_topicToOrdersDict.GetValueOrDefault(result.Topic) == null)
                _topicToOrdersDict.Add(result.Topic, new HashSet<string>());
            _topicToOrdersDict.GetValueOrDefault(result.Topic)?.Add(order.OrderId);
        }
    }

    private void HandleUpdateOrder(ConsumeResult<string, string> result)
    {
        var updateJson = result.Message.Value;
        Logger.Information($"[{Thread.CurrentThread.Name}]: JSON Update {updateJson}");
        if (updateJson != null)
        {
            var update = JsonConvert.DeserializeObject<UpdateOrder>(updateJson);
            Logger.Information($"[{Thread.CurrentThread.Name}]: OrderID: {update.OrderId}");
            Logger.Information($"[{Thread.CurrentThread.Name}]: Order Old Status: {_orders.GetValueOrDefault(update.OrderId)?.Status}");
            Logger.Information($"[{Thread.CurrentThread.Name}]: Order New Status: {update.Status}");
            this.UpdateOrderStatus(update.OrderId, update.Status);
            if (_topicToOrdersDict.GetValueOrDefault(result.Topic) == null)
                _topicToOrdersDict.Add(result.Topic, new HashSet<string>());
            _topicToOrdersDict.GetValueOrDefault(result.Topic)?.Add(update.OrderId);
        }
    }

}