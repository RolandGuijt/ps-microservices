using NServiceBus;

namespace GloboTicket.Integration.Messages;

public class ShipOrder: ICommand
{
    public Guid OrderId { get; set; }
    
}