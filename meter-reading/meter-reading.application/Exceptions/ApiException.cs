using System.Net;

namespace meter_reading.Application.Exceptions
{
    public class ApiException : HttpRequestException
    {
        public ApiException(string message, Exception inner, HttpStatusCode statusCode) : base(message, inner, statusCode)
        { }
    }
}
