public class UpdateOrder 
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public UpdateOrder(string orderId, string newStatus)
    {
        this.OrderId = orderId;
        this.Status = newStatus;
    }
}