using meter_reading.Application.CsvMappers;
using meter_reading.Application.Exceptions;
using meter_reading.Application.Extenstions;
using meter_reading.Application.Filters;
using meter_reading.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace meter_reading.Tests.Filters
{
    [TestFixture]
    public class ProcessCsvAttributeTests
    {
        private static IFormFile CreateFormFileFromString(string content, string name = "file", string fileName = "file.csv")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, name, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };
        }

        private static ActionExecutingContext CreateActionExecutingContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        }

        [Test]
        public void OnActionExecuting_WithValidCsv_SetsCsvItemOnHttpContext()
        {
            // Arrange
            var csv = new StringBuilder();
            csv.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");
            csv.AppendLine($"1,29/08/2008 09:54,123");
            csv.AppendLine($"2,12/07/2008 22:54,456");

            var context = new DefaultHttpContext();
            var file = CreateFormFileFromString(csv.ToString());
            var files = new FormFileCollection { file };
            context.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), files);

            var actionContext = CreateActionExecutingContext(context);

            var attribute = new ProcessCsvAttribute<MeterReading, MeterReadingMap>();

            // Act
            attribute.OnActionExecuting(actionContext);

            // Assert
            var items = context.GetCsvItem<List<MeterReading>>();
            Assert.That(items, Is.Not.Null);
            Assert.That(items!.Count, Is.EqualTo(2));
            Assert.That(items[0].MeterReadValue, Is.EqualTo(123));
            Assert.That(items[1].MeterReadValue, Is.EqualTo(456));
        }

        [Test]
        public void OnActionExecuting_WithHeaderOnlyCsv_ThrowsBadRequestException()
        {
            // Arrange: CSV with header but no data rows
            var csv = "AccountId,MeterReadingDateTime,MeterReadValue\n";

            var context = new DefaultHttpContext();
            var file = CreateFormFileFromString(csv);
            var files = new FormFileCollection { file };
            context.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), files);

            var actionContext = CreateActionExecutingContext(context);

            var attribute = new ProcessCsvAttribute<MeterReading, MeterReadingMap>();

            // Act & Assert
            var ex = Assert.Throws<BadRequestException>(() => attribute.OnActionExecuting(actionContext));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Is.EqualTo("CSV file contains no records."));
        }
    }
}