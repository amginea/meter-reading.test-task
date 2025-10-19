using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace web_api_db.Core.Services
{
    public class MeterReadingsService : IService<MeterReading>
    {
        private readonly IRepository<MeterReading> _meterReadingsRepository;
        private readonly IRepository<Account> _accountRepository;

        public MeterReadingsService(IRepository<MeterReading> meterReadingsRepository, IRepository<Account> accountRepository)
        {
            if (meterReadingsRepository == null)
                throw new ArgumentNullException(nameof(meterReadingsRepository));

            if (accountRepository == null)
                throw new ArgumentNullException(nameof(accountRepository));

            _meterReadingsRepository = meterReadingsRepository;
            _accountRepository = accountRepository;
        }

        public async Task<int> Upload(List<MeterReading> newMeterReadings)
        {
            throw new Exception("Test Exception from MeterReadingsService");
            var meterReadings = newMeterReadings
                .Select(_ => new { _.AccountId, _.MeterReadingDateTime, _.MeterReadValue })
                .Distinct();

            var existingAccounts = await _accountRepository
                .Get(_ => meterReadings
                    .Select(s => s.AccountId)
                    .Contains(_.AccountId))
                .Select(s => s.AccountId)
                .ToListAsync();

            meterReadings = meterReadings
                .Where(mr => mr.MeterReadValue >= 0 && mr.MeterReadValue <= 99999 && existingAccounts.Contains(mr.AccountId))
                .ToList();

            var existingMeterReadings = await _meterReadingsRepository
                .Get(_ => meterReadings.Select(mr => mr.AccountId).Contains(_.AccountId))
                .Select(s => new { s.AccountId, s.MeterReadingDateTime, s.MeterReadValue })
                .GroupBy(_ => _.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    Readings = g.OrderByDescending(mr => mr.MeterReadingDateTime).Select(mr => mr),
                }).ToListAsync();

            var readingsToSave = meterReadings
                .Where(newReading => !existingMeterReadings.Any(emr => emr.Readings.Contains(newReading)))
                .Where(newReading => existingMeterReadings
                    .Any(emr => emr.AccountId == newReading.AccountId && emr.Readings?
                        .FirstOrDefault()?.MeterReadingDateTime < newReading.MeterReadingDateTime))
                .Select(s => new MeterReading
                {
                    AccountId = s.AccountId,
                    MeterReadingDateTime = s.MeterReadingDateTime,
                    MeterReadValue = s.MeterReadValue
                }).ToList();

            if (readingsToSave.Any())
            {
                await _meterReadingsRepository.AddRangeAsync(readingsToSave);
                await _meterReadingsRepository.SaveChangesAsync();
            }

            return readingsToSave.Count;
        }
    }
}
