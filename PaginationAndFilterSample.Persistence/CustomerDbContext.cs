using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PaginationAndFilterSample.Domain;

namespace PaginationAndFilterSample.Persistence
{
    public class CustomerDbContext: DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(GetLoggerFactory())
                .UseSqlServer(
                    "Server = (localdb)\\mssqllocaldb; Database = PaginationSample; Trusted_Connection = True; ")
                .EnableSensitiveDataLogging(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entityType.Name).Property<DateTime>("Created");
                modelBuilder.Entity(entityType.Name).Property<DateTime>("LastModified");
            }
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            var timestamp = DateTime.Now;
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                entry.Property("LastModified").CurrentValue = timestamp;

                if (entry.State == EntityState.Added)
                {
                    entry.Property("Created").CurrentValue = timestamp;
                }
            }
            return base.SaveChanges();
        }
        private ILoggerFactory GetLoggerFactory()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder =>
                builder.AddConsole()
                    .AddFilter(DbLoggerCategory.Database.Command.Name,
                        LogLevel.Information));
            return serviceCollection.BuildServiceProvider()
                .GetService<ILoggerFactory>();
        }
    }
}