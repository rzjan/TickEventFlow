using MediatR;

namespace Ticketing.Query.Features.Tickets;

public sealed class TicketCreate
{
    public record CreateTicketCommand(
            string ID, 
            string Username, 
            int? TicketType, 
            string DetailError) : IRequest<string>;
}
