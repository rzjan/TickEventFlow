using Common.Core.Events;

namespace Ticketing.Command.Domain.Abstract;

public abstract class AggregateRoot
{
    protected string _id = string.Empty;
    public string Id 
    { 
        get { return _id; } 
    }
    public int Version { get; set; }

    private readonly List<BaseEvent> _changes = new();

    public IEnumerable<BaseEvent> GetUnCommitedChanges()
    {
        return _changes;
    }
    public void MarkChangesAsCommited()
    {
        _changes.Clear();
    }

    

    public void ApplyChange(BaseEvent @event, bool isNew = false)
    {
        var method = this.GetType().GetMethod("Apply", new Type[] { @event.GetType() });
        if (method == null)
        {
            throw new InvalidOperationException($"The Apply method for event type {@event.GetType().Name} was not found in aggregate {this.GetType().Name}.");
        }
        method.Invoke(this, [@event]);
        if (isNew)
        {
            _changes.Add(@event);
        }
    }

    public void RaiseEvent(BaseEvent @event)
    {
        ApplyChange(@event, true);
        _changes.Add(@event);
    }

}
