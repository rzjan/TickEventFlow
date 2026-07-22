using Common.Core.Events;

namespace Ticketing.Query.Domain.Abstractions;

public interface IEventHandler
{
    Task On(TicketCreatedEvent @event);

    //TODO: Implement the following event handler when the TicketUpdateEvent is defined
    Task On(TicketUpdatedEvent @event);
}
