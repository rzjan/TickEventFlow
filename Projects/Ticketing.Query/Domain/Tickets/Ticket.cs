using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Domain.TicketTypes;

namespace Ticketing.Query.Domain.Tickets;

public class Ticket: Entity
{
    public string? Description { get; set; }

    //Esta propiedad representa la relación entre Ticket y TicketType.
    //Se carga por lazy loading, lo que significa que el TicketType
    //asociado se cargará automáticamente desde la base de datos
    //cuando se acceda a esta propiedad.
    public virtual TicketType? TicketType { get; set; }

}
