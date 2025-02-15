using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Contorllers;

[ApiController]
[Route("order")]
public class OrderController : ControllerBase
{
    private readonly KafkaConsumer _consumer;

    public OrderController(KafkaConsumer consumer)
    {

        _consumer = consumer;
    }

    [HttpGet("order-details")]
    public IActionResult GetOrderDetails(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest("Invalid input for orderId");
        }
        var order = _consumer.GetOrder(orderId);
        if (order == null) return NotFound("Order not found.");
        Console.WriteLine(order);
        return Ok(order);
    }

    [HttpGet("getAllOrderIdsFromTopic")]
    public IActionResult GetAllOrderIdFromTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return BadRequest("Invalid input for Topic Name");
        }
        var orders = _consumer.GetOrdersIdFromTopicName(topic);
        if (orders == null) return NotFound("Order not found.");
        Console.WriteLine(orders);
        return Ok(orders);
    }
}
