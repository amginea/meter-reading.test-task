using meter_reading.Application.CsvMappers;
using meter_reading.Application.Filters;
using meter_reading.Application.Middlewares;
using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using web_api_db.Core.Repositories;
using web_api_db.Core.Repository;
using web_api_db.Core.Services;
using web_api_db.Infrastructure.Database;

namespace meter_reading.presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<ProcessCsvAttribute<MeterReading, MeterReadingMap>>();
            builder.Services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            });
            builder.Services.AddDbContext<DatabaseContext>(options => 
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddScoped<IRepository<MeterReading>, MeterReadingsRepository>();
            builder.Services.AddScoped<IRepository<Account>, AccountRepository>();
            builder.Services.AddScoped<IService<MeterReading>, MeterReadingsService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseExceptionHandler("/error");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCsvUploadValidator();

            app.MapControllers();

            app.Run();
        }
    }
}
