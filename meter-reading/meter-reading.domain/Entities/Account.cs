namespace meter_reading.Domain.Entities
{
    public class Account : BaseEntity
    {
        public int AccountId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
    }
}
