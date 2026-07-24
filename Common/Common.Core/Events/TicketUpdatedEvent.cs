namespace Common.Core.Events;

public class TicketUpdatedEvent: BaseEvent
{
    public TicketUpdatedEvent() : base(nameof(TicketUpdatedEvent))
    {
    }

    public int TicketType { get; set; }
    public string? Description { get; set; }
    public string? Username { get; set; }

}
