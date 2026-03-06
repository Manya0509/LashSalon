// LashBooking.Infrastructure/Repositories/GenericRepository.cs
using LashBooking.Domain.Interfaces;  // Ссылаемся на интерфейс из Domain
using LashBooking.Infrastructure.Data; // Ссылаемся на DbContext из Infrastructure
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LashBooking.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(int id)  // Асинхронно получает сущность по первичному ключу.
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync() // Асинхронно возвращает все записи из таблицы.
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) // Асинхронно ищет сущности, соответствующие условию predicate.
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(T entity) // Асинхронно добавляет новую сущность в базу.
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void Update(T entity) // Обновляет сущность
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(T entity) // Удаляет сущность.
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task SaveChangesAsync() // Асинхронно сохраняет все изменения в базе данных.
        {
            await _context.SaveChangesAsync();
        }
    }
}