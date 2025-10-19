using meter_reading.Application.Repository;
using meter_reading.Domain.Entities;
using meter_reading.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace meter_reading.Tests.Repository
{
    [TestFixture]
    public class AccountRepositoryTests
    {
        private DatabaseContext _context;
        private AccountRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Arrange - Create in-memory database with unique name for each test
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DatabaseContext(options);
            _repository = new AccountRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void Get_WhenFilteringByAccountId_ReturnsMatchingAccount()
        {
            // Arrange
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.AddRange(account1, account2);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.AccountId == 1).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].AccountId, Is.EqualTo(1));
            Assert.That(result[0].FirstName, Is.EqualTo("John"));
            Assert.That(result[0].LastName, Is.EqualTo("Doe"));
        }

        [Test]
        public void Get_WhenFilteringByFirstName_ReturnsMatchingAccounts()
        {
            // Arrange
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "John",
                LastName = "Smith",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account3 = new Account
            {
                AccountId = 3,
                FirstName = "Jane",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.AddRange(account1, account2, account3);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.FirstName == "John").ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(a => a.FirstName == "John"), Is.True);
        }

        [Test]
        public void Get_WhenFilteringByLastName_ReturnsMatchingAccounts()
        {
            // Arrange
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "Jane",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account3 = new Account
            {
                AccountId = 3,
                FirstName = "Bob",
                LastName = "Smith",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.AddRange(account1, account2, account3);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.LastName == "Doe").ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(a => a.LastName == "Doe"), Is.True);
        }

        [Test]
        public void Get_WhenNoMatchingRecords_ReturnsEmptyList()
        {
            // Arrange
            var account = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.AccountId == 999).ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Get_WhenDatabaseIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            // Database is empty (no data added)

            // Act
            var result = _repository.Get(a => a.AccountId == 1).ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Get_WithComplexPredicate_ReturnsMatchingAccounts()
        {
            // Arrange
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "John",
                LastName = "Smith",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account3 = new Account
            {
                AccountId = 3,
                FirstName = "Jane",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.AddRange(account1, account2, account3);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.FirstName == "John" && a.LastName == "Doe").ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].AccountId, Is.EqualTo(1));
            Assert.That(result[0].FirstName, Is.EqualTo("John"));
            Assert.That(result[0].LastName, Is.EqualTo("Doe"));
        }

        [Test]
        public void Get_ReturnsIQueryable_CanBeChainedWithOrderBy()
        {
            // Arrange
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account3 = new Account
            {
                AccountId = 3,
                FirstName = "Bob",
                LastName = "Johnson",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.AddRange(account1, account2, account3);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.AccountId > 1)
                .OrderBy(a => a.FirstName)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].FirstName, Is.EqualTo("Bob"));
            Assert.That(result[1].FirstName, Is.EqualTo("Jane"));
        }

        [Test]
        public void Get_ReturnsIQueryable_CanBeChainedWithTake()
        {
            // Arrange
            var accounts = Enumerable.Range(1, 10).Select(i => new Account
            {
                AccountId = i,
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            }).ToList();

            _context.Accounts.AddRange(accounts);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.AccountId > 0)
                .Take(5)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(5));
        }

        [Test]
        public async Task SaveChangesAsync_WhenDataIsModified_PersistsChanges()
        {
            // Arrange
            var account = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Modify the account
            var trackedAccount = _context.Accounts.First(a => a.AccountId == 1);
            trackedAccount.FirstName = "Jane";
            trackedAccount.LastName = "Smith";

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var savedAccount = _context.Accounts.First(a => a.AccountId == 1);
            Assert.That(savedAccount.FirstName, Is.EqualTo("Jane"));
            Assert.That(savedAccount.LastName, Is.EqualTo("Smith"));
        }

        [Test]
        public async Task SaveChangesAsync_WhenNoChanges_CompletesSuccessfully()
        {
            // Arrange
            var account = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _repository.SaveChangesAsync());
        }

        [Test]
        public async Task SaveChangesAsync_WhenMultipleEntitiesModified_PersistsAllChanges()
        {
            // Arrange
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.AddRange(account1, account2);
            await _context.SaveChangesAsync();

            // Modify both accounts
            var trackedAccount1 = _context.Accounts.First(a => a.AccountId == 1);
            var trackedAccount2 = _context.Accounts.First(a => a.AccountId == 2);
            trackedAccount1.FirstName = "UpdatedJohn";
            trackedAccount2.FirstName = "UpdatedJane";

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var savedAccount1 = _context.Accounts.First(a => a.AccountId == 1);
            var savedAccount2 = _context.Accounts.First(a => a.AccountId == 2);
            Assert.That(savedAccount1.FirstName, Is.EqualTo("UpdatedJohn"));
            Assert.That(savedAccount2.FirstName, Is.EqualTo("UpdatedJane"));
        }

        [Test]
        public void Get_WithMeterReadingsRelation_ReturnsAccountWithRelatedData()
        {
            // Arrange
            var account = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var meterReading1 = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 1,
                MeterReadingDateTime = DateTime.UtcNow.AddDays(-1),
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var meterReading2 = new MeterReading
            {
                MeterReadingId = 2,
                AccountId = 1,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12350,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            _context.MeterReadings.AddRange(meterReading1, meterReading2);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(a => a.AccountId == 1)
                .Include(a => a.MeterReadings)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].MeterReadings, Has.Count.EqualTo(2));
            Assert.That(result[0].MeterReadings.All(mr => mr.AccountId == 1), Is.True);
        }

        [Test]
        public void Get_WhenFilteringByDateRange_ReturnsMatchingAccounts()
        {
            // Arrange
            var baseDate = DateTime.UtcNow;
            var account1 = new Account
            {
                AccountId = 1,
                FirstName = "John",
                LastName = "Doe",
                Created = baseDate.AddDays(-10),
                Updated = baseDate.AddDays(-10)
            };
            var account2 = new Account
            {
                AccountId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Created = baseDate.AddDays(-5),
                Updated = baseDate.AddDays(-5)
            };
            var account3 = new Account
            {
                AccountId = 3,
                FirstName = "Bob",
                LastName = "Johnson",
                Created = baseDate.AddDays(-1),
                Updated = baseDate.AddDays(-1)
            };

            _context.Accounts.AddRange(account1, account2, account3);
            _context.SaveChanges();

            // Act
            var cutoffDate = baseDate.AddDays(-6);
            var result = _repository.Get(a => a.Created >= cutoffDate).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Any(a => a.AccountId == 2), Is.True);
            Assert.That(result.Any(a => a.AccountId == 3), Is.True);
        }
    }
}