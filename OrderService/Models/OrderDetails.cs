namespace OrderService.Models;

public class OrderDetails
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public double TotalAmount { get; set; }
    public double ShippingCost { get; set; }
    public List<string> Items { get; set; }
}
