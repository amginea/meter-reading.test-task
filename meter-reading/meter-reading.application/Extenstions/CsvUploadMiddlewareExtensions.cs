using meter_reading.Application.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace meter_reading.Application.Extenstions
{
    public static class CsvUploadMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsvUploadValidator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsvUploadValidatorMiddleware>();
        }
    }
}
