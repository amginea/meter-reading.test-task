using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using meter_reading.Application.Services;

namespace meter_reading.Tests.Services
{
    [TestFixture]
    public class MeterReadingsServiceTests
    {
        [Test]
        public void Constructor_WhenMeterReadingsRepositoryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var accountRepoMock = new Mock<IRepository<Account>>();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new MeterReadingsService(null!, accountRepoMock.Object));

            // Assert
            Assert.That(ex!.ParamName, Is.EqualTo("meterReadingsRepository"));
        }

        [Test]
        public void Constructor_WhenAccountRepositoryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var meterRepoMock = new Mock<IRepository<MeterReading>>();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new MeterReadingsService(meterRepoMock.Object, null!));

            // Assert
            Assert.That(ex!.ParamName, Is.EqualTo("accountRepository"));
        }

        [Test]
        public async Task Upload_WhenNoExistingAccounts_ReturnsZero_AndDoesNotCallAddRange()
        {
            // Arrange
            var meterRepoMock = new Mock<IRepository<MeterReading>>();
            var accountRepoMock = new Mock<IRepository<Account>>();

            var inputs = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadValue = 50, MeterReadingDateTime = DateTime.UtcNow }
            };

            // Return empty list as async queryable for accounts
            var emptyAccounts = new List<Account>();
            accountRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns((Expression<Func<Account, bool>> pred) =>
                    emptyAccounts.BuildMock());

            // Return empty list as async queryable for meter readings
            var emptyMeterReadings = new List<MeterReading>();
            meterRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<MeterReading, bool>>>()))
                .Returns((Expression<Func<MeterReading, bool>> pred) =>
                    emptyMeterReadings.BuildMock());

            var service = new MeterReadingsService(meterRepoMock.Object, accountRepoMock.Object);

            // Act
            var result = await service.Upload(inputs);

            // Assert
            Assert.That(result, Is.EqualTo(0));
            meterRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<ICollection<MeterReading>>()), Times.Never);
            meterRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task Upload_WithValidNewReading_AddsRangeAndReturnsCount()
        {
            // Arrange
            var meterRepoMock = new Mock<IRepository<MeterReading>>();
            var accountRepoMock = new Mock<IRepository<Account>>();

            var testDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

            // Existing account and existing meter reading (older) for AccountId = 1
            var existingAccounts = new List<Account>
            {
                new Account { AccountId = 1 }
            };

            var existingMeterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    AccountId = 1,
                    MeterReadingDateTime = testDate.AddDays(-1),
                    MeterReadValue = 10
                }
            };

            // Incoming has a duplicate row (should be distincted) and one valid newer reading
            var inputs = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadValue = 100, MeterReadingDateTime = testDate },
                new() { AccountId = 1, MeterReadValue = 100, MeterReadingDateTime = testDate }
            };

            accountRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns((Expression<Func<Account, bool>> pred) =>
                    existingAccounts.BuildMock());

            meterRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<MeterReading, bool>>>()))
                .Returns((Expression<Func<MeterReading, bool>> pred) =>
                    existingMeterReadings.BuildMock());

            ICollection<MeterReading>? captured = null;
            meterRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<ICollection<MeterReading>>()))
                .Callback<ICollection<MeterReading>>(c => captured = c)
                .Returns(Task.CompletedTask);

            meterRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new MeterReadingsService(meterRepoMock.Object, accountRepoMock.Object);

            // Act
            var savedCount = await service.Upload(inputs);

            // Assert
            Assert.That(savedCount, Is.EqualTo(1));
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Count, Is.EqualTo(1));
            var saved = captured.First();
            Assert.That(saved.AccountId, Is.EqualTo(1));
            Assert.That(saved.MeterReadValue, Is.EqualTo(100));
            Assert.That(saved.MeterReadingDateTime, Is.EqualTo(testDate));

            meterRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<ICollection<MeterReading>>()), Times.Once);
            meterRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Upload_WithInvalidMeterReadValue_FiltersOutInvalidReadings()
        {
            // Arrange
            var meterRepoMock = new Mock<IRepository<MeterReading>>();
            var accountRepoMock = new Mock<IRepository<Account>>();

            var testDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

            var existingAccounts = new List<Account>
            {
                new Account { AccountId = 1 }
            };

            var existingMeterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    AccountId = 1,
                    MeterReadingDateTime = testDate.AddDays(-1),
                    MeterReadValue = 100
                }
            };

            var inputs = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadValue = -1, MeterReadingDateTime = testDate },
                new() { AccountId = 1, MeterReadValue = 100000, MeterReadingDateTime = testDate },
                new() { AccountId = 1, MeterReadValue = 99999, MeterReadingDateTime = testDate },
            };

            accountRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns((Expression<Func<Account, bool>> pred) =>
                    existingAccounts.BuildMock());

            meterRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<MeterReading, bool>>>()))
                .Returns((Expression<Func<MeterReading, bool>> pred) =>
                    existingMeterReadings.BuildMock());

            ICollection<MeterReading>? captured = null;
            meterRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<ICollection<MeterReading>>()))
                .Callback<ICollection<MeterReading>>(c => captured = c)
                .Returns(Task.CompletedTask);

            meterRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new MeterReadingsService(meterRepoMock.Object, accountRepoMock.Object);

            // Act
            var savedCount = await service.Upload(inputs);

            // Assert
            Assert.That(savedCount, Is.EqualTo(1));
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Count, Is.EqualTo(1));
            Assert.That(captured.First().MeterReadValue, Is.EqualTo(99999));
        }

        [Test]
        public async Task Upload_WithDuplicateReadings_SavesOnlyDistinctReadings()
        {
            // Arrange
            var meterRepoMock = new Mock<IRepository<MeterReading>>();
            var accountRepoMock = new Mock<IRepository<Account>>();

            var testDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

            var existingAccounts = new List<Account>
            {
                new Account { AccountId = 1 }
            };

            var existingMeterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    AccountId = 1,
                    MeterReadingDateTime = testDate.AddDays(-1),
                    MeterReadValue = 50
                }
            };

            var inputs = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadValue = 100, MeterReadingDateTime = testDate },
                new() { AccountId = 1, MeterReadValue = 100, MeterReadingDateTime = testDate },
                new() { AccountId = 1, MeterReadValue = 100, MeterReadingDateTime = testDate }
            };

            accountRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns((Expression<Func<Account, bool>> pred) =>
                    existingAccounts.BuildMock());

            meterRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<MeterReading, bool>>>()))
                .Returns((Expression<Func<MeterReading, bool>> pred) =>
                    existingMeterReadings.BuildMock());

            ICollection<MeterReading>? captured = null;
            meterRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<ICollection<MeterReading>>()))
                .Callback<ICollection<MeterReading>>(c => captured = c)
                .Returns(Task.CompletedTask);

            meterRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var service = new MeterReadingsService(meterRepoMock.Object, accountRepoMock.Object);

            // Act
            var savedCount = await service.Upload(inputs);

            // Assert
            Assert.That(savedCount, Is.EqualTo(1));
            Assert.That(captured!.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Upload_WithOlderReading_DoesNotSave()
        {
            // Arrange
            var meterRepoMock = new Mock<IRepository<MeterReading>>();
            var accountRepoMock = new Mock<IRepository<Account>>();

            var testDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

            var existingAccounts = new List<Account>
            {
                new Account { AccountId = 1 }
            };

            var existingMeterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    AccountId = 1,
                    MeterReadingDateTime = testDate,
                    MeterReadValue = 100
                }
            };

            // Try to upload an older reading
            var inputs = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadValue = 50, MeterReadingDateTime = testDate.AddDays(-1) }
            };

            accountRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns((Expression<Func<Account, bool>> pred) =>
                    existingAccounts.BuildMock());

            meterRepoMock
                .Setup(r => r.Get(It.IsAny<Expression<Func<MeterReading, bool>>>()))
                .Returns((Expression<Func<MeterReading, bool>> pred) =>
                    existingMeterReadings.BuildMock());

            var service = new MeterReadingsService(meterRepoMock.Object, accountRepoMock.Object);

            // Act
            var savedCount = await service.Upload(inputs);

            // Assert
            Assert.That(savedCount, Is.EqualTo(0));
            meterRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<ICollection<MeterReading>>()), Times.Never);
            meterRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}