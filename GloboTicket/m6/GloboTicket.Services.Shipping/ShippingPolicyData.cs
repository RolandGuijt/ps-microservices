namespace GloboTicket.Services.Shipping;

class ShippingPolicyData : ContainSagaData
{
    public Guid OrderId { get; set; }

    public bool IsOrderPlaced { get; set; }

    public bool IsOrderBilled { get; set; }
}
