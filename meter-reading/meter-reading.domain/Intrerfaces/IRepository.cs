using meter_reading.Domain.Entities;
using System.Linq.Expressions;

namespace meter_reading.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> Get(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task AddRangeAsync(ICollection<T> entities);
        Task SaveChangesAsync();
    }
}
