using Common.Core.Events;
using MongoDB.Driver;
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

    public async Task SaveEventsAsync(string aggregateId, IEnumerable<BaseEvent> events, int expectedVersion, CancellationToken cancellationToken)
    {
        //Validar cual fue el ultimo evento que se guardo en la base de datos
        var evenStream = await _eventModelRepository.FilterByAsync(doc => doc.AggregateIdentifier == aggregateId, cancellationToken);
        if (!evenStream.Any() && expectedVersion != 1 && evenStream.Last().Version != expectedVersion) 
        {
            throw new ArgumentException("Error de concurrencia");
        }
        var version = expectedVersion;
        
        foreach (var @event in events)
        {
            version++;
            @event.Version = version;
            var evenType = @event.GetType().Name;
            var eventModel = new EventModel
            {
                Timestamp = DateTime.UtcNow,
                AggregateIdentifier = aggregateId,
                Version = version,
                EventType = evenType,
                EventData = @event
            };               
            
            await AddEvenStore(eventModel, cancellationToken);
        }   

    }

    private async Task AddEvenStore(
        EventModel eventModel,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle session = await _eventModelRepository.BeginSessionAsync(cancellationToken);

        try
        {
            _eventModelRepository.BeginTransaction(session);
            await _eventModelRepository.InsertOneAsync(eventModel, session, cancellationToken);

            await _eventModelRepository.CommitTransactionAsync(session, cancellationToken);
            _eventModelRepository.DisposeSession(session);
        }
        catch (Exception)
        {

            await _eventModelRepository.RollbackTransactionAsync(session, cancellationToken);
            _eventModelRepository.DisposeSession(session);
        }
    }
}
