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

}
