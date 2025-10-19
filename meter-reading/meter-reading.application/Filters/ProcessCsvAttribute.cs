using CsvHelper;
using CsvHelper.Configuration;
using meter_reading.Application.Exceptions;
using meter_reading.Application.Extenstions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

namespace meter_reading.Application.Filters
{
    public class ProcessCsvAttribute<TRecordType, TMap> : ActionFilterAttribute
        where TRecordType : class
        where TMap : ClassMap<TRecordType>
    {
        private const string FORM_DATA_KEY = "file";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var csv = context.HttpContext.Request.Form.Files.GetFile(FORM_DATA_KEY);

            using var reader = new StreamReader(csv.OpenReadStream());
            using CsvReader csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap(typeof(TMap));

            var records = csvReader.GetRecords<TRecordType>().ToList();
            
            if (records.Any())
                context.HttpContext.SetCsvItem(records);
            else
                throw new BadRequestException("CSV file contains no records.");

            base.OnActionExecuting(context);
        }
    }
}
