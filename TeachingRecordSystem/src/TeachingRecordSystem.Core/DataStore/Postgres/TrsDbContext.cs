using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

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

    public DbSet<JourneyState> JourneyStates => Set<JourneyState>();

    public DbSet<User> Users => Set<User>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Qualification> Qualifications => Set<Qualification>();

    public DbSet<MandatoryQualification> MandatoryQualifications => Set<MandatoryQualification>();

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

    public void AddEvent(EventBase @event, DateTime? inserted = null)
    {
        Events.Add(Event.FromEventBase(@event, inserted));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
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
