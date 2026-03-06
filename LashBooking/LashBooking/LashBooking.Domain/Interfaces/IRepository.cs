using System.Linq.Expressions;

namespace LashBooking.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id); // Получить по ID
        Task<IEnumerable<T>> GetAllAsync(); // Получить все записи
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate); // Найти по условию

        Task AddAsync(T entity); // Добавить новую запись
        void Update(T entity); // Обновить существующую
        void Delete(T entity); // Удалить запись
        Task SaveChangesAsync(); // Сохранить изменения в БД
    }
}
