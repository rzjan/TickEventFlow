using Common.Core.Events;
using System.ComponentModel.Design;
using Ticketing.Command.Domain.Abstract;
using static Ticketing.Command.Features.Tickets.TicketCreate;

namespace Ticketing.Command.Application.Aggregates;

public class TicketAggregate : AggregateRoot
{
    public bool Active { get; private set; }
    public TicketAggregate()
    {
        //Constructor vacio para poder instanciar el AggregateRoot
    }
    public TicketAggregate(TicketCreateCommand command)
    {
        var ticketCreatedEvent = new TicketCreatedEvent
        {
            Id = command.Id,
            UserName = command.ticketCreateRequest.Username,
            TypeError = command.ticketCreateRequest.TypeError,
            DetailError = command.ticketCreateRequest.DetailError
        };

        //Encargado de setear el objeto al AggregateRoot
        RaiseEvent(ticketCreatedEvent);
    }

    public void Apply(TicketCreatedEvent @event)
    {
        _id = @event.Id;
        Active = true;
    }  
}
