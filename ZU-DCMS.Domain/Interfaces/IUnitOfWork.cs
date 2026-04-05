using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /* 
         * It's common to have a method to get repositories for specific entities.
         * This method allows you to retrieve a repository for a specific entity type T, where T must inherit from BaseEntity 
         */
        IRepository<T> Repository<T>() where T : BaseEntity;

        /*
         * This method is responsible for saving all changes made in the context to the database. It returns the number of state entries written to the database.
         * The method is asynchronous and accepts an optional CancellationToken to allow for cancellation of the operation if needed. 
         */
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // __ These methods are related to transaction management. They allow you to begin a transaction, commit it, or roll it back in case of an error. __ //
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
