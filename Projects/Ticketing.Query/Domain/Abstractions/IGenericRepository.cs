namespace Ticketing.Query.Domain.Abstractions;

public interface IGenericRepository<T> where T: class
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    void AddEntity(T Entity);
    void UpdateEntity(T Entity);
    void DeleteEntity(T entity);
}
