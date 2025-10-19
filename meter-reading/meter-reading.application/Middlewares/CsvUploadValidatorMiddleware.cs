using meter_reading.Application.Constants;
using meter_reading.Application.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace meter_reading.Application.Middlewares
{
    public class CsvUploadValidatorMiddleware
    {
        private readonly RequestDelegate _next;

        public CsvUploadValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (UpploadAPIs.ApiEndpoints.Values.Any(_ => context.Request.Path.StartsWithSegments(_)) && !context.Request.HasFormContentType)
                throw new BadRequestException("Csv file was not provided!");

            await _next(context);
        }
    }

    public static class CsvUploadMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsvUploadValidator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsvUploadValidatorMiddleware>();
        }
    }
}
