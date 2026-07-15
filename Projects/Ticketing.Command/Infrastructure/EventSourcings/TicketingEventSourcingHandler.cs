using Ticketing.Command.Application.Aggregates;
using Ticketing.Command.Domain.Abstract;

namespace Ticketing.Command.Infrastructure.EventSourcings
{
    public class TicketingEventSourcingHandler : IEventSourcingHandler<TicketAggregate>
    {
        public Task<TicketAggregate> GetByIdAsync(string aggregateId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SaveChanges(AggregateRoot aggregate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
