using meter_reading.Application.Repositories;
using meter_reading.Domain.Entities;
using meter_reading.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace meter_reading.Tests.Repository
{
    [TestFixture]
    public class MeterReadingsRepositoryTests
    {
        private DatabaseContext _context;
        private MeterReadingsRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Arrange - Create in-memory database with unique name for each test
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DatabaseContext(options);
            _repository = new MeterReadingsRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region AddAsync Tests

        [Test]
        public async Task AddAsync_WhenValidMeterReading_AddsToContext()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(meterReading);
            await _repository.SaveChangesAsync();

            // Assert
            var savedReading = await _context.MeterReadings.FirstOrDefaultAsync(mr => mr.MeterReadingId == 1);
            Assert.That(savedReading, Is.Not.Null);
            Assert.That(savedReading.MeterReadingId, Is.EqualTo(1));
            Assert.That(savedReading.AccountId, Is.EqualTo(100));
            Assert.That(savedReading.MeterReadValue, Is.EqualTo(12345));
        }

        [Test]
        public async Task AddAsync_WhenMultipleCallsWithoutSave_AddsAllToContext()
        {
            // Arrange
            var meterReading1 = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var meterReading2 = new MeterReading
            {
                MeterReadingId = 2,
                AccountId = 101,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 67890,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(meterReading1);
            await _repository.AddAsync(meterReading2);
            await _repository.SaveChangesAsync();

            // Assert
            var count = await _context.MeterReadings.CountAsync();
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task AddAsync_WithRelatedAccount_MaintainsRelationship()
        {
            // Arrange
            var account = new Account
            {
                AccountId = 100,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Account = account,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(meterReading);
            await _repository.SaveChangesAsync();

            // Assert
            var savedReading = await _context.MeterReadings
                .Include(mr => mr.Account)
                .FirstOrDefaultAsync(mr => mr.MeterReadingId == 1);

            Assert.That(savedReading, Is.Not.Null);
            Assert.That(savedReading.Account, Is.Not.Null);
            Assert.That(savedReading.Account.AccountId, Is.EqualTo(100));
            Assert.That(savedReading.Account.FirstName, Is.EqualTo("John"));
        }

        #endregion

        #region AddRangeAsync Tests

        [Test]
        public async Task AddRangeAsync_WhenValidCollection_AddsAllToContext()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-2),
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-1),
                    MeterReadValue = 12350,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            // Act
            await _repository.AddRangeAsync(meterReadings);
            await _repository.SaveChangesAsync();

            // Assert
            var count = await _context.MeterReadings.CountAsync();
            Assert.That(count, Is.EqualTo(3));

            var savedReadings = await _context.MeterReadings.ToListAsync();
            Assert.That(savedReadings.Any(mr => mr.MeterReadingId == 1), Is.True);
            Assert.That(savedReadings.Any(mr => mr.MeterReadingId == 2), Is.True);
            Assert.That(savedReadings.Any(mr => mr.MeterReadingId == 3), Is.True);
        }

        [Test]
        public async Task AddRangeAsync_WhenEmptyCollection_DoesNotThrowException()
        {
            // Arrange
            var emptyCollection = new List<MeterReading>();

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
            {
                await _repository.AddRangeAsync(emptyCollection);
                await _repository.SaveChangesAsync();
            });

            var count = await _context.MeterReadings.CountAsync();
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task AddRangeAsync_WhenLargeCollection_AddsAllSuccessfully()
        {
            // Arrange
            var meterReadings = Enumerable.Range(1, 100).Select(i => new MeterReading
            {
                MeterReadingId = i,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow.AddDays(-i),
                MeterReadValue = 10000 + i,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            }).ToList();

            // Act
            await _repository.AddRangeAsync(meterReadings);
            await _repository.SaveChangesAsync();

            // Assert
            var count = await _context.MeterReadings.CountAsync();
            Assert.That(count, Is.EqualTo(100));
        }

        #endregion

        #region Get Tests

        [Test]
        public void Get_WhenFilteringByMeterReadingId_ReturnsMatchingReading()
        {
            // Arrange
            var meterReading1 = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            var meterReading2 = new MeterReading
            {
                MeterReadingId = 2,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow.AddDays(1),
                MeterReadValue = 12350,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.MeterReadings.AddRange(meterReading1, meterReading2);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.MeterReadingId == 1).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].MeterReadingId, Is.EqualTo(1));
            Assert.That(result[0].MeterReadValue, Is.EqualTo(12345));
        }

        [Test]
        public void Get_WhenFilteringByAccountId_ReturnsAllMatchingReadings()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-2),
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-1),
                    MeterReadValue = 12350,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _context.MeterReadings.AddRange(meterReadings);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.AccountId == 100).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(mr => mr.AccountId == 100), Is.True);
        }

        [Test]
        public void Get_WhenFilteringByMeterReadValue_ReturnsMatchingReadings()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12350,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 102,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 15000,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _context.MeterReadings.AddRange(meterReadings);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.MeterReadValue > 12345 && mr.MeterReadValue < 14000).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].MeterReadValue, Is.EqualTo(12350));
        }

        [Test]
        public void Get_WhenFilteringByDateRange_ReturnsMatchingReadings()
        {
            // Arrange
            var baseDate = DateTime.UtcNow;
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = baseDate.AddDays(-10),
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 100,
                    MeterReadingDateTime = baseDate.AddDays(-5),
                    MeterReadValue = 12350,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 100,
                    MeterReadingDateTime = baseDate.AddDays(-1),
                    MeterReadValue = 12355,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _context.MeterReadings.AddRange(meterReadings);
            _context.SaveChanges();

            // Act
            var cutoffDate = baseDate.AddDays(-6);
            var result = _repository.Get(mr => mr.MeterReadingDateTime >= cutoffDate).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Any(mr => mr.MeterReadingId == 2), Is.True);
            Assert.That(result.Any(mr => mr.MeterReadingId == 3), Is.True);
        }

        [Test]
        public void Get_WhenNoMatchingRecords_ReturnsEmptyList()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.MeterReadings.Add(meterReading);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.AccountId == 999).ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Get_WhenDatabaseIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            // Database is empty (no data added)

            // Act
            var result = _repository.Get(mr => mr.AccountId == 100).ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Get_ReturnsIQueryable_CanBeChainedWithOrderBy()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-1),
                    MeterReadValue = 12350,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-3),
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12355,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _context.MeterReadings.AddRange(meterReadings);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.AccountId == 100)
                .OrderBy(mr => mr.MeterReadValue)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].MeterReadValue, Is.EqualTo(12345));
            Assert.That(result[1].MeterReadValue, Is.EqualTo(12350));
            Assert.That(result[2].MeterReadValue, Is.EqualTo(12355));
        }

        [Test]
        public void Get_ReturnsIQueryable_CanBeChainedWithTakeAndSkip()
        {
            // Arrange
            var meterReadings = Enumerable.Range(1, 10).Select(i => new MeterReading
            {
                MeterReadingId = i,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow.AddDays(-i),
                MeterReadValue = 10000 + i,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            }).ToList();

            _context.MeterReadings.AddRange(meterReadings);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.AccountId == 100)
                .OrderBy(mr => mr.MeterReadingId)
                .Skip(3)
                .Take(5)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(5));
            Assert.That(result[0].MeterReadingId, Is.EqualTo(4));
            Assert.That(result[4].MeterReadingId, Is.EqualTo(8));
        }

        [Test]
        public void Get_WithInclude_LoadsRelatedAccount()
        {
            // Arrange
            var account = new Account
            {
                AccountId = 100,
                FirstName = "John",
                LastName = "Doe",
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            _context.MeterReadings.Add(meterReading);
            _context.SaveChanges();

            // Act
            var result = _repository.Get(mr => mr.MeterReadingId == 1)
                .Include(mr => mr.Account)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Account, Is.Not.Null);
            Assert.That(result[0].Account.FirstName, Is.EqualTo("John"));
            Assert.That(result[0].Account.LastName, Is.EqualTo("Doe"));
        }

        #endregion

        #region GetByIdAsync Tests

        [Test]
        public async Task GetByIdAsync_WhenMeterReadingExists_ReturnsCorrectReading()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.MeterReadingId, Is.EqualTo(1));
            Assert.That(result.AccountId, Is.EqualTo(100));
            Assert.That(result.MeterReadValue, Is.EqualTo(12345));
        }

        [Test]
        public async Task GetByIdAsync_WhenMeterReadingDoesNotExist_ReturnsNull()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WhenDatabaseIsEmpty_ReturnsNull()
        {
            // Arrange
            // Database is empty (no data added)

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WhenMultipleReadingsExist_ReturnsCorrectOne()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 102,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 11111,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _context.MeterReadings.AddRange(meterReadings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(2);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.MeterReadingId, Is.EqualTo(2));
            Assert.That(result.AccountId, Is.EqualTo(101));
            Assert.That(result.MeterReadValue, Is.EqualTo(67890));
        }

        #endregion

        #region SaveChangesAsync Tests

        [Test]
        public async Task SaveChangesAsync_WhenDataIsModified_PersistsChanges()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync();

            // Modify the meter reading
            var trackedReading = await _context.MeterReadings.FirstAsync(mr => mr.MeterReadingId == 1);
            trackedReading.MeterReadValue = 99999;
            trackedReading.Updated = DateTime.UtcNow.AddHours(1);

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var savedReading = await _context.MeterReadings.FirstAsync(mr => mr.MeterReadingId == 1);
            Assert.That(savedReading.MeterReadValue, Is.EqualTo(99999));
        }

        [Test]
        public async Task SaveChangesAsync_WhenNoChanges_CompletesSuccessfully()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync();

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _repository.SaveChangesAsync());
        }

        [Test]
        public async Task SaveChangesAsync_WhenMultipleEntitiesModified_PersistsAllChanges()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _context.MeterReadings.AddRange(meterReadings);
            await _context.SaveChangesAsync();

            // Modify both readings
            var trackedReading1 = await _context.MeterReadings.FirstAsync(mr => mr.MeterReadingId == 1);
            var trackedReading2 = await _context.MeterReadings.FirstAsync(mr => mr.MeterReadingId == 2);
            trackedReading1.MeterReadValue = 11111;
            trackedReading2.MeterReadValue = 22222;

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var savedReading1 = await _context.MeterReadings.FirstAsync(mr => mr.MeterReadingId == 1);
            var savedReading2 = await _context.MeterReadings.FirstAsync(mr => mr.MeterReadingId == 2);
            Assert.That(savedReading1.MeterReadValue, Is.EqualTo(11111));
            Assert.That(savedReading2.MeterReadValue, Is.EqualTo(22222));
        }

        [Test]
        public async Task SaveChangesAsync_AfterAddAsync_PersistsNewEntity()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(meterReading);
            await _repository.SaveChangesAsync();

            // Assert
            var count = await _context.MeterReadings.CountAsync();
            Assert.That(count, Is.EqualTo(1));

            var savedReading = await _context.MeterReadings.FirstOrDefaultAsync();
            Assert.That(savedReading, Is.Not.Null);
            Assert.That(savedReading.MeterReadingId, Is.EqualTo(1));
        }

        [Test]
        public async Task SaveChangesAsync_AfterAddRangeAsync_PersistsAllEntities()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            // Act
            await _repository.AddRangeAsync(meterReadings);
            await _repository.SaveChangesAsync();

            // Assert
            var count = await _context.MeterReadings.CountAsync();
            Assert.That(count, Is.EqualTo(2));
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task FullWorkflow_AddQueryUpdateSave_WorksCorrectly()
        {
            // Arrange
            var meterReading = new MeterReading
            {
                MeterReadingId = 1,
                AccountId = 100,
                MeterReadingDateTime = DateTime.UtcNow,
                MeterReadValue = 12345,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };

            // Act - Add
            await _repository.AddAsync(meterReading);
            await _repository.SaveChangesAsync();

            // Act - Query
            var retrieved = await _repository.GetByIdAsync(1);
            Assert.That(retrieved, Is.Not.Null);

            // Act - Update
            retrieved.MeterReadValue = 54321;
            await _repository.SaveChangesAsync();

            // Assert
            var updated = await _repository.GetByIdAsync(1);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.MeterReadValue, Is.EqualTo(54321));
        }

        [Test]
        public async Task ComplexQuery_FilteringAndOrdering_WorksCorrectly()
        {
            // Arrange
            var baseDate = DateTime.UtcNow;
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = baseDate.AddDays(-5),
                    MeterReadValue = 12000,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 100,
                    MeterReadingDateTime = baseDate.AddDays(-3),
                    MeterReadValue = 12500,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 100,
                    MeterReadingDateTime = baseDate.AddDays(-1),
                    MeterReadValue = 13000,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 4,
                    AccountId = 101,
                    MeterReadingDateTime = baseDate.AddDays(-2),
                    MeterReadValue = 20000,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            await _repository.AddRangeAsync(meterReadings);
            await _repository.SaveChangesAsync();

            // Act
            var cutoffDate = baseDate.AddDays(-4);
            var result = _repository.Get(mr => mr.AccountId == 100 && mr.MeterReadingDateTime >= cutoffDate)
                .OrderByDescending(mr => mr.MeterReadingDateTime)
                .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].MeterReadingId, Is.EqualTo(3)); // Most recent
            Assert.That(result[1].MeterReadingId, Is.EqualTo(2));
        }

        #endregion
    }
}