using meter_reading.Application.Exceptions;
using meter_reading.Presentation.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace meter_reading.Presentation.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;
        public ErrorController(ILogger<ErrorController> logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        [Route("/error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Process()
        {
            string errorMessage = string.Empty;
            HttpStatusCode? statusCode = HttpStatusCode.InternalServerError;

            var exceptionHandlerFeature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();

            switch (exceptionHandlerFeature?.Error)
            {
                case ApiException apiException:
                    statusCode = apiException.StatusCode;
                    errorMessage = apiException.Message;
                    break;
                default:
                    errorMessage = "An internal server error occurred.";
                    break;
            }

            var responseMessage = $"{statusCode} : {errorMessage}";

            _logger.LogError(responseMessage);

            return await Task.FromResult(StatusCode((int)statusCode, new ErrorResponse
            {
                ErrorMessage = responseMessage
            }));
        }
    }
}
