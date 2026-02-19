using NServiceBus;

namespace GloboTicket.Integration.Messages;

public record OrderPlaced(Guid OrderId, Guid BasketId, Guid UserId, int Total): IEvent {}