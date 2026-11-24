using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres;

public class BulkUpsertExtensionsTests : IAsyncLifetime
{
    private readonly string _schema = $"bulk_upsert_{Guid.NewGuid():N}";
    private readonly string _connectionString = TestDatabase.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        var sql = $"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE";
        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    [Fact]
    public async Task BulkUpsertAsync_NewEntities_InsertsRows()
    {
        // Arrange
        var dog = new Dog { AnimalId = Guid.NewGuid(), Name = "Rex", Tags = ["good", "boy"], BarkVolume = 7 };
        var cat = new Cat { AnimalId = Guid.NewGuid(), Name = "Tom", Tags = [], Lives = 9 };

        // Act
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.BulkUpsertAsync(new Animal[] { dog, cat });
        }

        // Assert
        await using var assertDbContext = CreateDbContext();
        var animals = await assertDbContext.Animals.ToDictionaryAsync(a => a.AnimalId);
        Assert.Equal(2, animals.Count);
        var insertedDog = Assert.IsType<Dog>(animals[dog.AnimalId]);
        Assert.Equal("Rex", insertedDog.Name);
        Assert.Equal(["good", "boy"], insertedDog.Tags);
        Assert.Equal(7, insertedDog.BarkVolume);
        var insertedCat = Assert.IsType<Cat>(animals[cat.AnimalId]);
        Assert.Equal("Tom", insertedCat.Name);
        Assert.Empty(insertedCat.Tags);
        Assert.Equal(9, insertedCat.Lives);
    }

    [Fact]
    public async Task BulkUpsertAsync_ExistingEntity_UpdatesRow()
    {
        // Arrange
        var animalId = Guid.NewGuid();

        await using (var dbContext = CreateDbContext())
        {
            await dbContext.BulkUpsertAsync([new Dog { AnimalId = animalId, Name = "Rex", Tags = ["old"], BarkVolume = 1 }]);
        }

        // Act
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.BulkUpsertAsync([new Dog { AnimalId = animalId, Name = "Max", Tags = ["new"], BarkVolume = 10 }]);
        }

        // Assert
        await using var assertDbContext = CreateDbContext();
        var dog = Assert.IsType<Dog>(Assert.Single(await assertDbContext.Animals.ToListAsync()));
        Assert.Equal("Max", dog.Name);
        Assert.Equal(["new"], dog.Tags);
        Assert.Equal(10, dog.BarkVolume);
    }

    [Fact]
    public async Task BulkUpsertAsync_DerivedType_WritesDiscriminatorFromModel()
    {
        // Arrange
        var cat = new Cat { AnimalId = Guid.NewGuid(), Name = "Tom", Tags = [], Lives = 9 };

        // Act
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.BulkUpsertAsync([cat]);
        }

        // Assert
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"SELECT kind FROM \"{_schema}\".animals WHERE animal_id = @id", connection);
        command.Parameters.AddWithValue("id", cat.AnimalId);
        var kind = await command.ExecuteScalarAsync();
        Assert.Equal((int)AnimalKind.Cat, kind);
    }

    [Fact]
    public async Task BulkUpsertAsync_PropertyWithValueConverter_WritesConvertedValue()
    {
        // Arrange
        var dog = new Dog { AnimalId = Guid.NewGuid(), Name = "Rex", Tags = ["good", "boy"], BarkVolume = 7 };

        // Act
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.BulkUpsertAsync([dog]);
        }

        // Assert
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"SELECT tags FROM \"{_schema}\".animals WHERE animal_id = @id", connection);
        command.Parameters.AddWithValue("id", dog.AnimalId);
        var tags = await command.ExecuteScalarAsync();
        Assert.Equal("good,boy", tags);
    }

    [Fact]
    public void BulkUpsert_SyncOverload_InsertsRows()
    {
        // Arrange
        var dog = new Dog { AnimalId = Guid.NewGuid(), Name = "Rex", Tags = [], BarkVolume = 3 };

        // Act
        using (var dbContext = CreateDbContext())
        {
            dbContext.BulkUpsert([dog]);
        }

        // Assert
        using var assertDbContext = CreateDbContext();
        var inserted = Assert.IsType<Dog>(Assert.Single(assertDbContext.Animals.ToList()));
        Assert.Equal(dog.AnimalId, inserted.AnimalId);
    }

    [Fact]
    public async Task BulkUpsertAsync_UnmappedType_Throws()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        // Act
        var exception = await Record.ExceptionAsync(() => dbContext.BulkUpsertAsync(new[] { new object() }));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    private TestDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(_connectionString).UseSnakeCaseNamingConvention().Options, _schema);

    private enum AnimalKind
    {
        Dog = 1,
        Cat = 2
    }

    private abstract class Animal
    {
        public required Guid AnimalId { get; init; }
        public required string Name { get; init; }
        public required List<string> Tags { get; init; }
        public AnimalKind Kind { get; }
    }

    private sealed class Dog : Animal
    {
        public required int BarkVolume { get; init; }
    }

    private sealed class Cat : Animal
    {
        public required int Lives { get; init; }
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options, string schema) : DbContext(options)
    {
        public string Schema => schema;

        public DbSet<Animal> Animals => Set<Animal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(schema);

            modelBuilder.Entity<Animal>(b =>
            {
                b.ToTable("animals");
                b.HasKey(a => a.AnimalId);
                b.HasDiscriminator(a => a.Kind)
                    .HasValue<Dog>(AnimalKind.Dog)
                    .HasValue<Cat>(AnimalKind.Cat);
                b.Property(a => a.Tags).HasConversion(
                    v => string.Join(',', v),
                    v => v.Length == 0 ? new List<string>() : v.Split(',', StringSplitOptions.None).ToList(),
                    new ValueComparer<List<string>>(
                        (a, b) => a!.SequenceEqual(b!),
                        v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                        v => v.ToList()));
            });

            modelBuilder.Entity<Dog>();
            modelBuilder.Entity<Cat>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, SchemaModelCacheKeyFactory>();
    }

    private sealed class SchemaModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) =>
            (context.GetType(), ((TestDbContext)context).Schema, designTime);
    }
}
