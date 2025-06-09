using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using OpenIddict.EntityFrameworkCore.Models;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.EntityFramework;
using TeachingRecordSystem.Core.Services.Webhooks;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public class TrsDbContext : DbContext
{
    private readonly IServiceProvider? _serviceProvider;

    public TrsDbContext(DbContextOptions<TrsDbContext> options, IServiceProvider serviceProvider)
        : base(options)
    {
        _serviceProvider = serviceProvider;
    }

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

    public DbSet<RouteToProfessionalStatusType> RoutesToProfessionalStatus => Set<RouteToProfessionalStatusType>();

    public DbSet<OutboxMessageProcessorMetadata> OutboxMessageProcessorMetadata => Set<OutboxMessageProcessorMetadata>();

    public DbSet<RouteToProfessionalStatus> ProfessionalStatuses => Set<RouteToProfessionalStatus>();

    public DbSet<DegreeType> DegreeTypes => Set<DegreeType>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<PreviousName> PreviousNames => Set<PreviousName>();

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
            .ConfigureWarnings(b =>
            {
                b.Throw(CoreEventId.NavigationLazyLoading);
                b.Throw(CoreEventId.DetachedLazyLoadingWarning);
                b.Throw(CoreEventId.LazyLoadOnDisposedContextWarning);
            });
    }

    public async Task AddEventAndBroadcastAsync(EventBase @event)
    {
        Events.Add(Event.FromEventBase(@event, inserted: null));

        _ = _serviceProvider ?? throw new InvalidOperationException("No ServiceProvider on DbContext.");
        var webhookMessageFactory = _serviceProvider.GetRequiredService<WebhookMessageFactory>();
        var messages = await webhookMessageFactory.CreateMessagesAsync(this, @event, _serviceProvider);
        WebhookMessages.AddRange(messages);
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
}
