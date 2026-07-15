using Common.Core.Events;

namespace Ticketing.Command.Domain.Abstract;

public interface IEventStore
{
    Task<List<BaseEvent>>GetEventsAsync(
        string aggregateId, 
        CancellationToken cancellationToken);

    Task SaveEventsAsync(
        string aggregateId, 
        IEnumerable<BaseEvent> events, 
        int expectedVersion,
        CancellationToken cancellationToken);
}
