using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Core;
using HandmadeProductManagement.Repositories.Context;
using System.Linq.Expressions;

namespace HandmadeProductManagement.Repositories.UOW
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DatabaseContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(DatabaseContext dbContext)
        {
            _context = dbContext;
            _dbSet = _context.Set<T>();
        }
        public IQueryable<T> Entities => _context.Set<T>();

        public void Delete(object id)
        {
            T entity = _dbSet.Find(id) ?? throw new Exception();
            _dbSet.Remove(entity);
        }

        public async Task DeleteAsync(object id)
        {
            T entity = await _dbSet.FindAsync(id) ?? throw new Exception();
            _dbSet.Remove(entity);
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.AsEnumerable();
        }

        public async Task<IList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public T? GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<BasePaginatedList<T>> GetPagging(IQueryable<T> query, int index, int pageSize)
        {
            query = query.AsNoTracking();
            int count = await query.CountAsync();
            IReadOnlyCollection<T> items = await query.Skip((pageSize - 1) * pageSize).Take(pageSize).ToListAsync();
            return new BasePaginatedList<T>(items, count, index, pageSize);
        }

        public void Insert(T obj)
        {
            _dbSet.Add(obj);
        }

        public async Task InsertAsync(T obj)
        {
            await _dbSet.AddAsync(obj);
        }

        public void InsertRange(IList<T> obj)
        {
            _dbSet.AddRange(obj);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<T?> FindAsync(params object[] keyValues) => await _dbSet.FindAsync(keyValues);
        public void Update(T obj)
        {
            _dbSet.Entry(obj).State = EntityState.Modified;
        }

        public Task UpdateAsync(T obj)
        {
            return Task.FromResult(_dbSet.Update(obj));
        }

        public async Task<IList<T>> SearchAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }

    }
}
