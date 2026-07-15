using MongoDB.Driver;
using System.Linq.Expressions;
using Ticketing.Command.Domain.Common;

namespace Ticketing.Command.Domain.Abstract;

public interface IMongoRepository<TDocument> :ISession where TDocument: IDocument
    
{
    IQueryable<TDocument> AsQueryable();

    Task InsertOneAsync(
        TDocument document,
        IClientSessionHandle clientSessionHandle,
        CancellationToken cancellationToken
    );        

    Task InsertOneAsync(
        TDocument document,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<TDocument>> FilterByAsync(
                Expression<Func<TDocument, bool>> filterExpression, 
                CancellationToken cancellationToken);
}
