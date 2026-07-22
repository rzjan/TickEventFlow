using System.Collections;
using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Domain.Employees;
using Ticketing.Query.Infrastructure.Repositories;

namespace Ticketing.Query.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private Hashtable _repositories = new();
    private readonly TicketDbContext _context;
    private readonly DataBaseContextFactory _contextFactory;
    private IEmployeeRepository? _employeeRepository;

    public IEmployeeRepository EmployeeRepository => _employeeRepository ??= new EmployeeRepository(_context);

    public UnitOfWork(DataBaseContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
        _context = _contextFactory.CreateDbContext();
    }

    public async Task<int> Complete()
    {
        return await _context.SaveChangesAsync();
    }

    public IGenericRepository<TEntity> RepositoryGeneric<TEntity>() where TEntity : class
    {
        if(_repositories is null)
        {
            _repositories = new Hashtable();
        }
        var type = typeof(TEntity).Name;

        if (!_repositories.Contains(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);
            _repositories.Add(type, repositoryInstance);
        }
        return (IGenericRepository<TEntity>)_repositories[type]!;
        
    }
}
