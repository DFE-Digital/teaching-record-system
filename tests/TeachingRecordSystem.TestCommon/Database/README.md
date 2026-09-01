# Pooled test databases

See [docs/test-databases.md](../../../docs/test-databases.md) for the full picture, including how to migrate
a project and why the design is shaped this way. This file covers the mechanics of the code in this folder.

Gives every test its own Postgres database, so tests can run concurrently and still assert over whole tables
(counts, "list all", search ordering) without seeing each other's data.

```
Postgres (the existing testcontainer or configured server)
  └── template database        migrated + reference data seeded, once per schema
        └── pool of databases  cloned from the template, grown lazily
              └── one lease per test, reset on return
```

## Using it in a test project

1. The fixture initialises the pool and points the host at it:

   ```csharp
   await TestDatabases.InitializeAsync();          // in the assembly fixture's InitializeAsync
   services.AddPooledTestDatabase();               // last, in ConfigureServices, so it wins
   ```

   Fixtures deriving from `ServiceProviderFixture` get both for free.

2. The test base class leases a database per test, either by deriving from `PooledDatabaseTestBase` or by
   calling `TestDatabases.AcquireAsync` from `InitializeAsync` and disposing the lease afterwards.

3. Delete any `[ClearDbBeforeTest]` attributes and `DisableParallelization` collections. An exclusive
   database per test makes both unnecessary.

A failing test's database is kept, with its data, and named in the test output so it can be inspected with
`psql`. It is dropped from the pool rather than reused.

## What each project uses

| Project | Status |
|---|---|
| `Core.Tests` | Database per test, `parallelMode: all` |
| `SupportUi.Tests` | Database per test |
| `Api.IntegrationTests` | Database per test |
| `Api.UnitTests` | Database per test |
| `AuthorizeAccess.Tests` | Database per test |
| `Cli.Tests` | Database per test, connection string overlaid onto the command's configuration |
| `SupportUi.EndToEndTests` | One database for the whole run (real Kestrel, so no ambient scope) |
| `EndToEndTests` | One database for the whole run |

`DbHelper` still owns the testcontainer and the `trs` database; no project runs its tests against `trs` any
more, but it remains the thing that starts the container.

## Things worth knowing before migrating another project

- **Ambient state must not be set in `InitializeAsync`.** A value assigned to an `AsyncLocal` inside an async
  method is invisible to its caller, so the test body would never see it. `TestScopedServices` gets away with
  it only because it is set from a *constructor*. `TestDatabaseScope` keys off `TestContext` instead.
- **Don't register the pool's `NpgsqlDataSource` in DI.** The container disposes what it hands out, so a
  closing request scope disposes a data source the pool still owns. `AddPooledTestDatabase` keeps the
  DI-built `DbContextOptions` (so `PublishEventsDbCommandInterceptor` stays attached) and swaps only the
  connection.
- **Process-wide caches of database content break.** `ReferenceDataCache` is a singleton; once each test has
  its own database, a test that adds reference data publishes ids that don't exist anywhere else, and
  concurrent tests fail on a foreign key. `PooledReferenceDataCaches` keeps one cache per database. Anything
  else caching rows process-wide needs the same treatment.
- **Reference tables that tests write to can't simply be preserved.** `training_providers` is truncated and
  restored from a snapshot taken when the template was built; immutable ones are left alone.
- **Cache keys must cover everything that determines the result.** The pooled database name includes a hash
  of the schema *and* the reset statement, and the cached table list is keyed on the model assembly *and*
  the table classifications. Both of these caused real bugs when they only covered the schema.
