using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public class TrsDbContext : DbContext
{
    public TrsDbContext(DbContextOptions<TrsDbContext> options)
        : base(options)
    {
    }

    public static TrsDbContext Create(string connectionString, int? commandTimeout = null) =>
        new TrsDbContext(CreateOptions(connectionString, commandTimeout));

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

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<OneLoginUser> OneLoginUsers => Set<OneLoginUser>();

    public DbSet<NameSynonyms> NameSynonyms => Set<NameSynonyms>();

    public DbSet<PersonSearchAttribute> PersonSearchAttributes => Set<PersonSearchAttribute>();

    public DbSet<Establishment> Establishments => Set<Establishment>();

    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string connectionString, int? commandTimeout = null)
    {
        Action<NpgsqlDbContextOptionsBuilder> configureOptions = o => o.CommandTimeout(commandTimeout);

        if (connectionString != null)
        {
            optionsBuilder.UseNpgsql(connectionString, configureOptions);
        }
        else
        {
            optionsBuilder.UseNpgsql(configureOptions);
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

    private static DbContextOptions<TrsDbContext> CreateOptions(string connectionString, int? commandTimeout)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrsDbContext>();
        ConfigureOptions(optionsBuilder, connectionString, commandTimeout);
        return optionsBuilder.Options;
    }
}
