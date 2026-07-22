using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Domain.Employees;
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
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<TicketEmployee> TicketEmployees { get; set; } = new List<TicketEmployee>();

    private Ticket(){}
    private Ticket(Guid id, TicketType? ticketType, string description):base(id)
    {
        TicketType = ticketType;
        Description = description;

    }

    public static Ticket Create(Guid id, TicketType? ticketType, string description)
    {
        return new Ticket(id, ticketType, description);
    }
}
