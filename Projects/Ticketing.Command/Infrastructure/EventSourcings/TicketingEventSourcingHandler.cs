using Ticketing.Command.Application.Aggregates;
using Ticketing.Command.Domain.Abstract;

namespace Ticketing.Command.Infrastructure.EventSourcings
{
    public class TicketingEventSourcingHandler : IEventSourcingHandler<TicketAggregate>
    {
        //Inyectamos el EvenStore
        private readonly IEventStore _eventStore;

        public TicketingEventSourcingHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<TicketAggregate> GetByIdAsync(string aggregateId, CancellationToken cancellationToken)
        {
            //El objetivo del metodo es buscar el ultimo estado del aggregate,
            //para eso necesitamos obtener todos los eventos que se han generado para ese aggregateId

            var aggregate = new TicketAggregate();
            var events = await _eventStore.GetEventsAsync(aggregateId, cancellationToken);

            if (events is null || !events.Any()) { return aggregate; }
            aggregate.ReplayEvents(events);
            aggregate.Version = events.Select(x=> x.Version).Max();
            return aggregate;
        }

        public async Task SaveChanges(AggregateRoot aggregate, CancellationToken cancellationToken)
        {
            //El objetivo del metodo es guardar los eventos que se han generado en el aggregate al Store
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUnCommitedChanges(), aggregate.Version, cancellationToken);
            aggregate.MarkChangesAsCommited();
        }
    }
}
