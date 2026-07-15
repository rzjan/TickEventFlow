using Common.Core.Events;
using Ticketing.Command.Domain.Abstract;
using Ticketing.Command.Domain.EventModels;

namespace Ticketing.Command.Infrastructure.Persistence;

public class EventStore : IEventStore
{
    private readonly IEventModelRepository _eventModelRepository;

    public EventStore(IEventModelRepository eventModelRepository)
    {
        _eventModelRepository = eventModelRepository;
    }

    public async Task<List<BaseEvent>> GetEventsAsync(string aggregateId, CancellationToken cancellationToken)
    {
        
        var eventStream = await _eventModelRepository.FilterByAsync(doc => doc.AggregateIdentifier == aggregateId, cancellationToken);

        if (eventStream == null || !eventStream.Any()) 
        {
            throw new ArgumentException("NO existe eventos");
        }  

        return eventStream.OrderBy(x=> x.Version).Select(x=> x.EventData).ToList()!;

    }

    public Task SaveEventsAsync(string aggregateId, IEnumerable<BaseEvent> events, int expectedVersion, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
