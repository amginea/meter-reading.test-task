# meter-reading.test-task

## Overview
A small .NET 9 solution that accepts CSV meter reading uploads, validates and parses them, and persists readings. The API exposes a minimal controller surface and includes middleware, filters, and centralized error handling. Unit tests use NUnit and Moq.

## Projects
- meter-reading.domain — Domain entities (e.g. MeterReading, Account).
- meter-reading.application — Application logic: exceptions, CSV processing, filters, middleware, HttpContext extensions and services.
- meter-reading.infrastructure — Persistence layer (repositories, DbContext).
- meter-reading.presentation — ASP.NET Core Web API (controllers, Program.cs, error pipeline).
- meter-reading.Tests — NUnit test project covering controllers, services, filters, middleware and repositories.

## Requirements
- .NET 9 SDK
- Visual Studio (use __Build > Build Solution__ or the CLI)
- Tests use NUnit and Moq (already referenced in the test project)

## Build
From the solution root:

```bash
dotnet build
```

Or in Visual Studio use __Build > Build Solution__.

## Run (presentation API)
From the presentation project folder:

```bash
cd meter-reading.presentation dotnet run
```

Or set `meter-reading.presentation` as the startup project in Visual Studio and use __Debug > Start Debugging__ or __Run without Debugging__.

## Important Endpoints
- POST /api/v1/meter-readings/uploads
  - Implemented in MeterReadingController.Uploads.
  - Decorated with a CSV-processing attribute/filter that parses multipart file input into a List<MeterReading>.
  - Upload endpoints are validated by CsvUploadValidatorMiddleware to ensure requests use form content.

- Error endpoint (internal):
  - /error — handled by ErrorController to produce consistent ErrorResponse payloads.

## Upload (example)
Send a multipart/form-data request with a CSV file (fields expected by MeterReadingMap: AccountId, MeterReadingDateTime, MeterReadValue):

Example using curl:
```bash
curl -v -F "file=@/path/to/readings.csv" http://localhost:5000/api/v1/meter-readings/uploads
```

## Error handling
- ApiException-derived exceptions (e.g., BadRequestException) map to HTTP status codes.
- ErrorController reads IExceptionHandlerFeature and returns an ErrorResponse containing Status + Message and a TraceId.

## Tests
Run tests from solution root:

```bash
dotnet test
```