namespace meter_reading.Presentation.Responses
{
    public class ErrorResponse
    {
        public ErrorResponse()
        {
            TraceId = Guid.NewGuid();
        }

        public Guid TraceId { get; private set; }
        public string ErrorMessage { get; set; }
    }
}
