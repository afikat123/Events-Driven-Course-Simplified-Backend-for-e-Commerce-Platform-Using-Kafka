namespace OrderService.Models;

public class Order
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public double TotalAmount { get; set; }
    public List<string> Items { get; set; }
}
