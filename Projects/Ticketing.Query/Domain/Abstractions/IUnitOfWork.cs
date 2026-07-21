namespace Ticketing.Query.Domain.Abstractions;

public interface IUnitOfWork
{
    IGenericRepository<T> RepositoryGeneric<T>() where T : class;
    Task<int> Complete();
}
