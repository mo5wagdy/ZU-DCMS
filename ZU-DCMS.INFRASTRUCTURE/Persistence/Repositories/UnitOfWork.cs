using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // __ Dictionary to hold repositories for different entity types __ //
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        /* 
         * Get the repository for the specified entity type.
         * If the repository doesn't exist in the dictionary, create a new instance and add it to the dictionary.
         * This allows us to reuse the same repository instance for the same entity type throughout the unit of work.
         */
        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
                _repositories[type] = new Repository<T>(_context);

            return (IRepository<T>)_repositories[type];
        }

        // ________ Save changes to the database ________ //
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);

        // ________ Transaction management methods ________ //
        public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction!.CommitAsync();
            }
            finally
            {
                await _transaction!.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction!.RollbackAsync();
            }
            finally
            {
                await _transaction!.DisposeAsync();
                _transaction = null;
            }
        }

        // ________ Dispose the context when done ________ //
        public void Dispose() => _context.Dispose();
    }
}
