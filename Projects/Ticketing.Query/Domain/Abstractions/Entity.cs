namespace Ticketing.Query.Domain.Abstractions;

public class Entity(Guid id)
{
    // Constructor por defecto para permitir la creación de instancias sin parámetros
    protected Entity() : this(default) { }
    public Guid Id { get; private set; } = id;
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime?LastModifiedOn { get; private set; }
    public string? LastModifiedBy { get; private set; }
}
