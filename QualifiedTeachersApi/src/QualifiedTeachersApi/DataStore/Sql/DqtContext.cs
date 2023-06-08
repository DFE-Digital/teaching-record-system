using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Events;

namespace QualifiedTeachersApi.DataStore.Sql;

public class DqtContext : DbContext
{
    public DqtContext(DbContextOptions<DqtContext> options)
        : base(options)
    {
    }

    public static DqtContext Create(string connectionString) =>
        new DqtContext(CreateOptions(connectionString));

    public DbSet<TrnRequest> TrnRequests => Set<TrnRequest>();

    public DbSet<EntityChangesJournal> EntityChangesJournals => Set<EntityChangesJournal>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<QtsAwardedEmailsJob> QtsAwardedEmailsJobs => Set<QtsAwardedEmailsJob>();

    public DbSet<QtsAwardedEmailsJobItem> QtsAwardedEmailsJobItems => Set<QtsAwardedEmailsJobItem>();

    public DbSet<InternationalQtsAwardedEmailsJob> InternationalQtsAwardedEmailsJobs => Set<InternationalQtsAwardedEmailsJob>();

    public DbSet<InternationalQtsAwardedEmailsJobItem> InternationalQtsAwardedEmailsJobItems => Set<InternationalQtsAwardedEmailsJobItem>();

    public DbSet<EytsAwardedEmailsJob> EytsAwardedEmailsJobs => Set<EytsAwardedEmailsJob>();

    public DbSet<EytsAwardedEmailsJobItem> EytsAwardedEmailsJobItems => Set<EytsAwardedEmailsJobItem>();

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DqtContext).Assembly);
    }

    private static DbContextOptions<DqtContext> CreateOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DqtContext>();
        ConfigureOptions(optionsBuilder, connectionString);
        return optionsBuilder.Options;
    }
}
