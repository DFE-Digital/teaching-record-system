using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public class TrsDbContext : DbContext
{
    public TrsDbContext(DbContextOptions<TrsDbContext> options)
        : base(options)
    {
    }

    public static TrsDbContext Create(string connectionString) =>
        new TrsDbContext(CreateOptions(connectionString));

    public DbSet<TrnRequest> TrnRequests => Set<TrnRequest>();

    public DbSet<EntityChangesJournal> EntityChangesJournals => Set<EntityChangesJournal>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<QtsAwardedEmailsJob> QtsAwardedEmailsJobs => Set<QtsAwardedEmailsJob>();

    public DbSet<QtsAwardedEmailsJobItem> QtsAwardedEmailsJobItems => Set<QtsAwardedEmailsJobItem>();

    public DbSet<InternationalQtsAwardedEmailsJob> InternationalQtsAwardedEmailsJobs => Set<InternationalQtsAwardedEmailsJob>();

    public DbSet<InternationalQtsAwardedEmailsJobItem> InternationalQtsAwardedEmailsJobItems => Set<InternationalQtsAwardedEmailsJobItem>();

    public DbSet<EytsAwardedEmailsJob> EytsAwardedEmailsJobs => Set<EytsAwardedEmailsJob>();

    public DbSet<EytsAwardedEmailsJobItem> EytsAwardedEmailsJobItems => Set<EytsAwardedEmailsJobItem>();

    public DbSet<InductionCompletedEmailsJob> InductionCompletedEmailsJobs => Set<InductionCompletedEmailsJob>();

    public DbSet<InductionCompletedEmailsJobItem> InductionCompletedEmailsJobItems => Set<InductionCompletedEmailsJobItem>();

    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        if (connectionString != null)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            optionsBuilder.UseNpgsql();
        }

        optionsBuilder
            .UseSnakeCaseNamingConvention();
    }

    public void AddEvent(EventBase @event)
    {
        Events.Add(Event.FromEventBase(@event));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrsDbContext).Assembly);
    }

    private static DbContextOptions<TrsDbContext> CreateOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrsDbContext>();
        ConfigureOptions(optionsBuilder, connectionString);
        return optionsBuilder.Options;
    }
}
