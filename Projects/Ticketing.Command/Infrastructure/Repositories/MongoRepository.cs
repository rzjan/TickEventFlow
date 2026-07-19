using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;
using Ticketing.Command.Application.Models;
using Ticketing.Command.Domain.Abstract;
using Ticketing.Command.Domain.Common;

namespace Ticketing.Command.Infrastructure.Repositories;

public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : IDocument
{
     private readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(
        IOptions<MongoSettings> mongoSettings,
        IMongoClient mongoClient)
    {
        _collection = mongoClient
            .GetDatabase(mongoSettings.Value.DatabaseName)
            .GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
    }

    private protected string GetCollectionName(Type documentType)
    {
        var collectionName = documentType
            .GetCustomAttributes(typeof(BsonCollectionAttribute), true)
            .FirstOrDefault();

        if (collectionName != null)
        {
            return ((BsonCollectionAttribute)collectionName).CollectionName;
        }

        throw new ArgumentException("La colección es desconocida");
    }

    public IQueryable<TDocument> AsQueryable()
    {
        return _collection.AsQueryable();
    }

    public async Task<IClientSessionHandle> BeginSessionAsync(CancellationToken cancellationToken)
    {
        var option = new ClientSessionOptions();
        option.DefaultTransactionOptions = new TransactionOptions();

        return await _collection.Database.Client.StartSessionAsync(option, cancellationToken);

    }

    public void BeginTransaction(IClientSessionHandle clientSessionHandle)
    {
        clientSessionHandle.StartTransaction();
    }

    public Task CommitTransactionAsync(IClientSessionHandle clientSessionHandle, CancellationToken cancellationToken)
    {
        return clientSessionHandle.CommitTransactionAsync(cancellationToken);
    }

    public void DisposeSession(IClientSessionHandle clientSessionHandle)
    {
        clientSessionHandle.Dispose();
    }

    public async Task InsertOneAsync(TDocument document, IClientSessionHandle clientSessionHandle, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(clientSessionHandle, document, null, cancellationToken);
    }

    public async Task InsertOneAsync(TDocument document, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(document, null, cancellationToken);
    }

    public async Task RollbackTransactionAsync(IClientSessionHandle clientSessionHandle, CancellationToken cancelToken)
    {
        await clientSessionHandle.AbortTransactionAsync(cancelToken);
    }

    public async Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression, CancellationToken cancellationToken)
    {
        var coleccion = await _collection.FindAsync(filterExpression, null, cancellationToken);
        var result = await coleccion.ToListAsync();
        return result.Count != 0 ? result : Enumerable.Empty<TDocument>();
    }
}
