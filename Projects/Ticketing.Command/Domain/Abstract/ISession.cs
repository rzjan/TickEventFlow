using MongoDB.Driver;

namespace Ticketing.Command.Domain.Abstract;

public interface ISession
{
    Task<IClientSessionHandle> BeginSessionAsync(
       CancellationToken cancellationToken
     );

    void BeginTransaction(IClientSessionHandle clientSessionHandle);

    Task CommitTransactionAsync(
        IClientSessionHandle clientSessionHandle, 
        CancellationToken cancellationToken
    );

    Task RollbackTransactionAsync(
        IClientSessionHandle clientSessionHandle,
        CancellationToken cancelToken
    );

    void DisposeSession(IClientSessionHandle clientSessionHandle);
}
