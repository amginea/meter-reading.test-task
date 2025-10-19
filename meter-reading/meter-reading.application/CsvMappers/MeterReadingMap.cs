using CsvHelper.Configuration;
using meter_reading.Domain.Entities;

namespace meter_reading.Application.CsvMappers
{
    public class MeterReadingMap : ClassMap<MeterReading>
    {
        private const string DATE_TIME_FORMAT = "dd/MM/yyyy HH:mm";

        public MeterReadingMap()
        {
            Map(m => m.AccountId);
            Map(m => m.MeterReadingDateTime).TypeConverterOption.Format(DATE_TIME_FORMAT);
            Map(m => m.MeterReadValue);
        }
    }
}
