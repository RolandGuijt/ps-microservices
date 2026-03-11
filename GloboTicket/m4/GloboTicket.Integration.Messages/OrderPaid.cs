using NServiceBus;

namespace GloboTicket.Integration.Messages;

public record OrderPaid(Guid OrderId): IEvent;