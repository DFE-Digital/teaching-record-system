using System.ComponentModel.DataAnnotations.Schema;
using static TeachingRecordSystem.AuthorizeAccess.IdModelTypes;

namespace TeachingRecordSystem.AuthorizeAccess;

public class IdDbContext(DbContextOptions<IdDbContext> options) : DbContext(options)
{
    public DbSet<IdTrnToken> TrnTokens => Set<IdTrnToken>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdTrnToken>().HasKey(t => t.TrnToken);
        modelBuilder.Entity<User>().HasKey(u => u.UserId);
    }
}

[Table("trn_tokens")]
public class IdTrnToken
{
    [Column("trn_token")]
    public required string TrnToken { get; set; }
    [Column("trn")]
    public required string Trn { get; set; }
    [Column("email")]
    public required string Email { get; set; }
    [Column("created_utc")]
    public required DateTime CreatedUtc { get; set; }
    [Column("expires_utc")]
    public required DateTime ExpiresUtc { get; set; }
    [Column("user_id")]
    public Guid? UserId { get; set; }
}

[Table("users")]
public class User
{
    [Column("user_id")]
    public Guid UserId { get; set; }
    [Column("email_address")]
    public required string EmailAddress { get; set; }
    [Column("first_name")]
    public required string FirstName { get; set; }
    [Column("last_name")]
    public required string LastName { get; set; }
    [Column("created")]
    public required DateTime Created { get; set; }
    [Column("updated")]
    public required DateTime Updated { get; set; }
    [Column("user_type")]
    public IdModelTypes.UserType UserType { get; set; }
    [Column("trn")]
    public string? Trn { get; set; }
    [Column("trn_association_source")]
    public TrnAssociationSource? TrnAssociationSource { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
    [Column("trn_lookup_support_ticket_created")]
    public bool TrnLookupSupportTicketCreated { get; set; }
    [Column("trn_verification_level")]
    public TrnVerificationLevel? TrnVerificationLevel { get; set; }
}

public static class IdModelTypes
{
    public enum UserType
    {
        Default = 0,
#pragma warning disable CA1069
        Teacher = 0,
#pragma warning restore CA1069
        Staff = 1
    }

    public enum TrnAssociationSource
    {
        Lookup = 0,
        Api = 1,
        SupportUi = 2,
        UserImport = 3,
        TrnToken = 4
    }

    public enum TrnVerificationLevel
    {
        Low = 0,
        Medium = 1
    }
}
