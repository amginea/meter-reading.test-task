namespace meter_reading.Application.Constants
{
    public static class UpploadAPIs
    {
        public static Dictionary<Guid, string> ApiEndpoints = new Dictionary<Guid, string>
        {
            { Guid.NewGuid(), "/api/v1/meter-readings/uploads" },
        };
    }
}
