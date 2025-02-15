using System.Text.Json.Serialization;

public class Order
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public double TotalAmount { get; set; }
    public List<string> Items { get; set; }

    public static Order Generate(string orderId, int itemsNum)
    {
        return new Order
        {
            OrderId = orderId,
            Status = "new",
            TotalAmount = new Random().NextDouble() * 100,
            Items = Enumerable.Range(1, itemsNum).Select(i => $"Item{i}").ToList()
        };
    }
}