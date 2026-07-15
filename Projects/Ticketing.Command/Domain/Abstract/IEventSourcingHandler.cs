namespace Ticketing.Command.Domain.Abstract;

public interface IEventSourcingHandler<T>
{
    Task<T> GetByIdAsync(string aggregateId, CancellationToken cancellationToken);
    Task SaveChanges(AggregateRoot aggregate, CancellationToken cancellationToken);
}
