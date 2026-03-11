using NServiceBus;

namespace GloboTicket.Integration.Messages;

public record PlaceOrder(Guid UserId, Guid BasketId, string CardNumber, int BasketTotal) : ICommand { }
