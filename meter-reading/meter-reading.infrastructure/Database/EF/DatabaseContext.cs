using meter_reading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace meter_reading.Infrastructure.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }

        public DatabaseContext() { }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer("");
            base.OnConfiguring(optionsBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                entry.Entity.Updated = DateTime.Now;

                if (entry.State == EntityState.Added)
                    entry.Entity.Created = DateTime.Now;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
