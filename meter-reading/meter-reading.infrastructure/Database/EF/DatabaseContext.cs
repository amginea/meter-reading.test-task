using meter_reading.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace web_api_db.Infrastructure.Database
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
                optionsBuilder.UseSqlServer("Data Source=meter-read.c3y6qffhz31h.us-east-1.rds.amazonaws.com;User ID=admin;Password=7nT1mDHnu1OVrXAslwqR;TrustServerCertificate=True;");
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
