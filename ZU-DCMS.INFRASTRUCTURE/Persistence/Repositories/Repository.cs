using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ZU_DCMS.Domain.Common;
using ZU_DCMS.Domain.Interfaces;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Repositories
{
    // Generic repository implementation for basic CRUD operations
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();

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
