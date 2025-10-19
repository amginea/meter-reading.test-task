using meter_reading.Domain.Entities;

namespace meter_reading.Domain.Interfaces
{
    public interface IService<T> where T : BaseEntity
    {
        Task<int> Upload(List<T> values);
    }
}
