using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using OpenIddict.EntityFrameworkCore.Models;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Infrastructure.EntityFramework;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public partial class TrsDbContext : DbContext
{
    public const string ConnectionName = "DefaultConnection";

    private TrsDbContext(DbContextOptions<TrsDbContext> options)
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

    public DbSet<MandatoryQualificationProvider> MandatoryQualificationProviders => Set<MandatoryQualificationProvider>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    public DbSet<OneLoginUser> OneLoginUsers => Set<OneLoginUser>();

    public DbSet<NameSynonyms> NameSynonyms => Set<NameSynonyms>();

    public DbSet<PersonSearchAttribute> PersonSearchAttributes => Set<PersonSearchAttribute>();

    public DbSet<Establishment> Establishments => Set<Establishment>();

    public DbSet<EstablishmentSource> EstablishmentSources => Set<EstablishmentSource>();

    public DbSet<TpsCsvExtract> TpsCsvExtracts => Set<TpsCsvExtract>();

    public DbSet<TpsCsvExtractLoadItem> TpsCsvExtractLoadItems => Set<TpsCsvExtractLoadItem>();

    public DbSet<TpsCsvExtractItem> TpsCsvExtractItems => Set<TpsCsvExtractItem>();

    public DbSet<TpsEmployment> TpsEmployments => Set<TpsEmployment>();

    public DbSet<SupportTask> SupportTasks => Set<SupportTask>();

    public DbSet<TpsEstablishment> TpsEstablishments => Set<TpsEstablishment>();

    public DbSet<TpsEstablishmentType> TpsEstablishmentTypes => Set<TpsEstablishmentType>();

    public DbSet<Alert> Alerts => Set<Alert>();

    public DbSet<AlertType> AlertTypes => Set<AlertType>();

    public DbSet<AlertCategory> AlertCategories => Set<AlertCategory>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();

    public DbSet<WebhookMessage> WebhookMessages => Set<WebhookMessage>();

    public DbSet<TrnRequestMetadata> TrnRequestMetadata => Set<TrnRequestMetadata>();

    public DbSet<InductionExemptionReason> InductionExemptionReasons => Set<InductionExemptionReason>();

    public DbSet<JobMetadata> JobMetadata => Set<JobMetadata>();

    public DbSet<TrainingProvider> TrainingProviders => Set<TrainingProvider>();

    public DbSet<TrainingSubject> TrainingSubjects => Set<TrainingSubject>();

    public DbSet<RouteToProfessionalStatusType> RouteToProfessionalStatusTypes => Set<RouteToProfessionalStatusType>();

    public DbSet<OutboxMessageProcessorMetadata> OutboxMessageProcessorMetadata => Set<OutboxMessageProcessorMetadata>();

    public DbSet<RouteToProfessionalStatus> RouteToProfessionalStatuses => Set<RouteToProfessionalStatus>();

    public DbSet<DegreeType> DegreeTypes => Set<DegreeType>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<PreviousName> PreviousNames => Set<PreviousName>();

    public DbSet<IntegrationTransaction> IntegrationTransactions => Set<IntegrationTransaction>();

    public DbSet<IntegrationTransactionRecord> IntegrationTransactionRecords => Set<IntegrationTransactionRecord>();

    public DbSet<Email> Emails => Set<Email>();

    public DbSet<TrnRange> TrnRanges => Set<TrnRange>();

    public DbSet<Process> Processes => Set<Process>();

    public DbSet<ProcessEvent> ProcessEvents => Set<ProcessEvent>();

    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string? connectionString = null, int? commandTimeout = null)
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
            .UseSnakeCaseNamingConvention()
            .UseOpenIddict<Guid>()
            .AddInterceptors(new PopulateOidcApplicationInterceptor())
            .UseProjectables()
            .UseSeeding((context, _) => ((TrsDbContext)context).SeedData())
            .UseAsyncSeeding((context, _, cancellationToken) => ((TrsDbContext)context).SeedDataAsync(cancellationToken))
            .ConfigureWarnings(b =>
            {
                b.Throw(CoreEventId.NavigationLazyLoading);
                b.Throw(CoreEventId.DetachedLazyLoadingWarning);
                b.Throw(CoreEventId.LazyLoadOnDisposedContextWarning);
            });
    }

    public void AddEventWithoutBroadcast(EventBase @event)
    {
        Events.Add(Event.FromEventBase(@event, inserted: null));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrsDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (clrType.Assembly == typeof(OpenIddictEntityFrameworkCoreApplication).Assembly)
            {
                entityType.SetTableName(clrType.Name.Split("`")[0] switch
                {
                    nameof(OpenIddictEntityFrameworkCoreApplication) => "oidc_applications",
                    nameof(OpenIddictEntityFrameworkCoreAuthorization) => "oidc_authorizations",
                    nameof(OpenIddictEntityFrameworkCoreScope) => "oidc_scopes",
                    nameof(OpenIddictEntityFrameworkCoreToken) => "oidc_tokens",
                    _ => throw new NotSupportedException($"Cannot configure table name for {clrType.Name}.")
                });
            }
        }
    }

    private static DbContextOptions<TrsDbContext> CreateOptions(string connectionString, int? commandTimeout)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrsDbContext>();
        ConfigureOptions(optionsBuilder, connectionString, commandTimeout);
        return optionsBuilder.Options;
    }

    public static TrsDbContext Create(NpgsqlDataSource dataSource, int? commandTimeout = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrsDbContext>();
        ConfigureOptions(optionsBuilder, connectionString: null, commandTimeout);
        var dbContext = new TrsDbContext(optionsBuilder.Options);
        dbContext.Database.SetDbConnection(dataSource.CreateConnection(), contextOwnsConnection: true);
        return dbContext;
    }

    public static TrsDbContext Create(DbConnection connection, int? commandTimeout = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrsDbContext>();
        ConfigureOptions(optionsBuilder, connectionString: null, commandTimeout);
        var dbContext = new TrsDbContext(optionsBuilder.Options);
        dbContext.Database.SetDbConnection(connection, contextOwnsConnection: false);
        return dbContext;
    }
}
