// Infrastructure.Persistence/Data/ApplicationDbContext.cs
using Framework.Domain.Primitives;
using Framework.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // <-- GetService<T>()
using Microsoft.Extensions.Logging.Abstractions;
using Web.Api.Domain.Companies;

namespace Web.Api.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
    {
        public DbSet<Company> Companies { get; set; } = default!;

        // ÚNICO ctor público, solo con opciones (requerido por AddDbContextPool)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // No guardes servicios en campos si usas pooling. Resuélvelos cuando se necesiten:
            IPublisher GetPublisher() => this.GetService<IPublisher>() ?? new NoopPublisher();

            ILogger<ApplicationDbContext> GetLogger() =>
                this.GetService<ILogger<ApplicationDbContext>>() ?? NullLogger<ApplicationDbContext>.Instance;

            List<DomainEvent> DequeueAllDomainEvents() =>
                ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .SelectMany(e => e.DomainEvents)
                .ToList();

            var pendingEvents = DequeueAllDomainEvents();

            if (pendingEvents.Count == 0)
                return await base.SaveChangesAsync(cancellationToken);

            var strategy = Database.CreateExecutionStrategy();
            int result = 0;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await Database.BeginTransactionAsync(cancellationToken);
                result = await base.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            });

            var publisher = GetPublisher();
            var logger = GetLogger();

            while (pendingEvents.Count > 0)
            {
                foreach (var domainEvent in pendingEvents)
                {
                    try
                    {
                        await publisher.Publish(domainEvent, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error publicando DomainEvent {EventType}: {@Event}",
                            domainEvent.GetType().Name, domainEvent);
                    }
                }

                foreach (var agg in ChangeTracker.Entries<AggregateRoot>().Select(e => e.Entity))
                    agg.ClearDomainEvents();

                pendingEvents = DequeueAllDomainEvents();
            }

            return result;
        }

        // Publisher no-op para escenarios donde no esté registrado (design-time, etc.)
        private sealed class NoopPublisher : IPublisher
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification => Task.CompletedTask;
        }
    }
}
