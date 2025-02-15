using System.Text.Json;
using System.Threading.Tasks;
using CartService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Core;
namespace CartService.Controllers;

[ApiController]
[Route("cart")]
public class CartController : ControllerBase
{
    private readonly KafkaProducer _producer;
    public CartController(KafkaProducer producer)
    {
        _producer = producer;
    }

    [HttpPost("create-order")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateOrder(string orderId, int itemsNum)
    {
        if (string.IsNullOrWhiteSpace(orderId) || itemsNum <= 0)
            return BadRequest("Invalid input for orderId or itemsNum.");

        if (_producer.isOrderExist(orderId))
            return BadRequest($"Order ID {orderId} already exist");

        var order = Order.Generate(orderId, itemsNum);
        try
        {
            await _producer.produceMessage(order, _producer.newOrdersTopicName);
            // await Task.Run(() => _producer.produceMessage(order, "new-orders-topic"));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        return Ok("Order published!");
    }

    [HttpPost("update-order")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateOrder(string orderId, string newStatus)
    {
        if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(newStatus))
            return BadRequest("Invalid input for orderId or newStatus.");

        if (!_producer.isOrderExist(orderId))
            return BadRequest($"Order ID {orderId} doesn't exist");

        var update = new UpdateOrder(orderId, newStatus);
        try
        {
            await _producer.produceMessage(update, _producer.updateOrdersTopicName);
            // await Task.Run(() => _producer.produceMessage(order, "new-orders-topic"));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        return Ok("Order Updated!");
    }
}
