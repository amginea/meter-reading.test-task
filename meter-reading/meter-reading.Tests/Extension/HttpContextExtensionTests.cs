using meter_reading.Application.Extenstions;
using meter_reading.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace meter_reading.Tests.Extensions
{
    [TestFixture]
    public class HttpContextExtensionTests
    {
        [Test]
        public void GetCsvItem_WhenNoItem_ReturnsNull()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = context.GetCsvItem<List<MeterReading>>();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SetCsvItem_WhenCalled_GetCsvItemReturnsSameInstance()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var items = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = 100, MeterReadingDateTime = DateTime.UtcNow }
            };

            // Act
            context.SetCsvItem(items);
            var result = context.GetCsvItem<List<MeterReading>>();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(items));
        }

        [Test]
        public void GetCsvItem_WhenStoredValueIsDifferentType_ReturnsNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Items["csv"] = "not-a-list";

            // Act
            var result = context.GetCsvItem<List<MeterReading>>();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SetCsvItem_WhenCalledTwice_OverwritesPreviousValue()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var first = new List<MeterReading> { new MeterReading { AccountId = 1 } };
            var second = new List<MeterReading> { new MeterReading { AccountId = 2 } };

            // Act
            context.SetCsvItem(first);
            context.SetCsvItem(second);
            var result = context.GetCsvItem<List<MeterReading>>();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(second));
            Assert.That(result![0].AccountId, Is.EqualTo(2));
        }
    }
}