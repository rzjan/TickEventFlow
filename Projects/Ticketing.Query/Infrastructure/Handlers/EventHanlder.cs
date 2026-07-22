using Common.Core.Events;
using MediatR;
using Ticketing.Query.Domain.Abstractions;
using static Ticketing.Query.Features.Tickets.TicketCreate;

namespace Ticketing.Query.Infrastructure.Handlers;

public class EventHanlder : IEventHandler
{
    private readonly IMediator _mediator;

    public EventHanlder(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task On(TicketCreatedEvent @event)
    {
        var command = new TicketCreateCommand(
                @event.Id,
                @event.UserName,
                @event.TypeError,
                @event.DetailError
            );

        await _mediator.Send( command );
    }

    public Task On(TicketUpdatedEvent @event)
    {
        throw new NotImplementedException();
    }
}
