using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingRecordSystem.AuthorizeAccess;

public class IdDbContext(DbContextOptions<IdDbContext> options) : DbContext(options)
{
    public DbSet<IdTrnToken> TrnTokens => Set<IdTrnToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdTrnToken>().HasKey(t => t.TrnToken);
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
