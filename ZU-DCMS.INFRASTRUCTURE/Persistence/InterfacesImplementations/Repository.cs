using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.InterfacesImplementations
{
    // __ Generic repository implementation for basic CRUD operations __ //
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        // __ Protected fields for the database context and DbSet __ //
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        // __ Constructor that initializes the database context and DbSet __ //
        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // __________ Implementation of IRepository<T> methods __________ //
        public async Task<T?> GetByIdAsync(int id, params string[] includes)
        {
            var query = _dbSet.AsQueryable();

            foreach (var include in includes) query = query.Include(include);

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, bool disabledTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (disabledTracking)
                query = query.AsNoTracking();

            foreach (var include in includes) query = query.Include(include);

            if (predicate != null) query = query.Where(predicate);

            return await query.ToListAsync();
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool disabledTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (disabledTracking)
                query = query.AsNoTracking();

            foreach (var include in includes) query = query.Include(include);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();

        // __ Method to get a paginated list of entities with optional filtering and including related entities __ //
        public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedListAsync(int skip, int take, Expression<Func<T, bool>>? predicate = null, bool disabledTracking = true, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] includes)
        {
            // __ Start with the base queryable from the DbSet __ //
            IQueryable<T> query = _dbSet.AsQueryable();

            // __ If tracking is disabled, use AsNoTracking for better performance __ //
            if (disabledTracking)
                query = query.AsNoTracking();

            // __ Include related entities as specified in the includes parameter __ //
            foreach (var include in includes) query = query.Include(include);

            // __ Apply the filtering predicate if provided __ //
            if (predicate != null) query = query.Where(predicate);

            // __ Get the total count of items matching the criteria before pagination __ //
            var totalCount = await query.CountAsync();

            // __ Apply ordering if provided __ //
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // __ Apply pagination using Skip and Take __ //
            var items = await query.Skip(skip).Take(take).ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public void Update(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public void Delete(T entity) => _dbSet.Remove(entity);

        public void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
    }
}
