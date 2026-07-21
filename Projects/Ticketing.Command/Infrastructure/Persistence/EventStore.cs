using Common.Core.Events;
using Common.Core.Producers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Ticketing.Command.Application.Models;
using Ticketing.Command.Domain.Abstract;
using Ticketing.Command.Domain.EventModels;

namespace Ticketing.Command.Infrastructure.Persistence;

public class EventStore : IEventStore
{
    private readonly IEventModelRepository _eventModelRepository;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IEventProducer _eventProducer;

    public EventStore(IEventModelRepository eventModelRepository, 
            IOptions<KafkaSettings> kafkaSettings,
            IEventProducer eventProducer)
    {
        _eventModelRepository = eventModelRepository;
        _kafkaSettings = kafkaSettings.Value;
        _eventProducer = eventProducer;
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
        // Validar concurrencia: comparar la última versión persistida con expectedVersion
        var eventStream = (await _eventModelRepository.FilterByAsync(doc => doc.AggregateIdentifier == aggregateId, cancellationToken)).ToList();

        if (!eventStream.Any())
        {
            // No hay eventos previos: esperamos que expectedVersion sea 0
            if (expectedVersion != 0)
            {
                throw new ArgumentException("Error de concurrencia");
            }
        }
        else
        {
            var lastVersion = eventStream.Max(x => x.Version);
            if (lastVersion != expectedVersion)
            {
                throw new ArgumentException("Error de concurrencia");
            }
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

            // Insert using transactions/sessions (requires replica set and correct DNS from the app)
            await AddEvenStore(eventModel, cancellationToken);
            var topic = _kafkaSettings.Topic ?? throw new ArgumentException("no se encontro el topic");
            await _eventProducer.ProduceAsync(topic, @event);
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
