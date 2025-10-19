namespace meter_reading.Domain.Entities
{
    public class MeterReading : BaseEntity
    {
        public int AccountId { get; set; }
        public int MeterReadingId { get; set; }
        public DateTime MeterReadingDateTime { get; set; }
        public int MeterReadValue { get; set; }
        public Account? Account { get; set; }
    }
}
