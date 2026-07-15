using Ticketing.Command.Domain.Abstract;

namespace Ticketing.Command.Domain.EventModels;

public interface IEventModelRepository: IMongoRepository<EventModel>
{
    
}
