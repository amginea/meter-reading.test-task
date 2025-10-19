using meter_reading.Application.CsvMappers;
using meter_reading.Application.Extenstions;
using meter_reading.Application.Filters;
using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using meter_reading.Presentation.Responses;
using Microsoft.AspNetCore.Mvc;

namespace meter_reading.Presentation.Controllers
{
    [Route("api/v1/meter-readings")]
    [ApiController]
    public class MeterReadingController : ControllerBase
    {
        private IService<MeterReading> _meterReadingService;

        public MeterReadingController(IService<MeterReading> meterReadinService)
        {
            if (meterReadinService == null)
                throw new ArgumentNullException(nameof(meterReadinService));

            _meterReadingService = meterReadinService;
        }

        [HttpPost("uploads")]
        [ProcessCsv<MeterReading, MeterReadingMap>]
        public async Task<IActionResult> Uploads()
        {
            var meterReadings = HttpContext.GetCsvItem<List<MeterReading>>(); // Retrieved from Csv Processing Filter
            var savedMeterReadings = await _meterReadingService.Upload(meterReadings);

            return Ok(new UploadResponse
            {
                Success = savedMeterReadings,
                Failed = meterReadings.Count - savedMeterReadings
            });
        }
    }
}
