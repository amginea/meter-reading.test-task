using Microsoft.AspNetCore.Http;

namespace meter_reading.Application.Extenstions
{
    public static class HttpContextExtension
    {
        private const string CSV_KEY = "csv";

        public static T? GetCsvItem<T>(this HttpContext context) where T : class
        {
            if (context.Items.ContainsKey(CSV_KEY))
                return context.Items[CSV_KEY] as T;
            return default;
        }

        public static void SetCsvItem<T>(this HttpContext context, T value) where T : class
        {
            context.Items[CSV_KEY] = value;
        }
    }
}
