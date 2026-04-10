using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ZU_DCMS.Domain.Common;

namespace ZU_DCMS.Domain.Interfaces
{
    /*
     * Generic repository interface for basic CRUD operations
     * T is constrained to be a type that inherits from BaseEntity,
     * ensuring that all entities have a common base structure
     */
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, params string[] includes); // => Asynchronous method to retrieve an entity by its unique identifier, with optional related entities to include in the query
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool disabledTracking = true, params Expression<Func<T, object>>[] includes ); // => Asynchronous method to retrieve the first entity that matches a given predicate, or null if no such entity exists
        Task<IEnumerable<T>> GetAllAsync(); // => Asynchronous method to retrieve all entities of type T
        Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, bool disabledTracking = true, params Expression<Func<T, object>>[] includes); // => Asynchronous method to retrieve a list of entities that match a given predicate, with optional related entities to include in the query; if no predicate is provided, all entities are retrieved
        Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedListAsync(int skip, int take, Expression<Func<T, bool>>? predicate = null, bool disabledTracking = true, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] includes); // => Asynchronous method to retrieve a paged list of entities that match a given predicate, with optional related entities to include in the query; if no predicate is provided, all entities are retrieved
        IQueryable<T> GetQueryable(); // => Method to retrieve an IQueryable of type T, allowing for further querying and deferred execution
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate); // => Asynchronous method to check if any entities match a given predicate, returning true if at least one entity matches
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null); // => Asynchronous method to count the number of entities that match a given predicate, or count all entities if no predicate is provided
        Task AddAsync(T entity); // => Asynchronous method to add a new entity to the repository
        Task AddRangeAsync(IEnumerable<T> entities); // => Asynchronous method to add a range of entities to the 
        void Update(T entity); // => Method to update an existing entity in the repository
        void Delete(T entity); // => Method to delete an entity from the repository
        void SoftDelete(T entity); // => Method to perform a soft delete on an entity, marking it as deleted without removing it from the database
    }
}
