using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using meter_reading.Infrastructure.Database;

namespace meter_reading.Application.Repositories
{
    public class MeterReadingsRepository : IRepository<MeterReading>
    {
        private readonly DatabaseContext _context;

        public MeterReadingsRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MeterReading entity)
        {
            await _context.MeterReadings.AddAsync(entity);
        }

        public async Task AddRangeAsync(ICollection<MeterReading> entities)
        {
            await _context.MeterReadings.AddRangeAsync(entities);
        }

        public IQueryable<MeterReading> Get(Expression<Func<MeterReading, bool>> predicate)
        {
            return _context.MeterReadings.Where(predicate);
        }

        public async Task<MeterReading?> GetByIdAsync(int id)
        {
            var result = await _context.MeterReadings.Where(mr => mr.MeterReadingId == id).FirstOrDefaultAsync();
            return result;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
