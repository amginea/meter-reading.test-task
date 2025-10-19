namespace meter_reading.Application.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message, Exception inner = null) : base(message, inner, System.Net.HttpStatusCode.BadRequest)
        {
        }
    }
}
