# Test databases

This document describes how database-backed tests get a database, why the design is what it is, and what to
do when migrating a test project onto it.

## The design

Every database-backed test project uses pooled databases. Which shape it gets depends on whether the code
under test can see the running test's ambient scope.

### A database per test

Used by `Core.Tests`, `SupportUi.Tests`, `Api.IntegrationTests`, `Api.UnitTests`, `AuthorizeAccess.Tests`
and `Cli.Tests`.

```
Postgres (testcontainer, or whatever ConnectionStrings:DefaultConnection points at)
  └── trs_tmpl_<schema>_v<n>_<seeds>   migrated + seeded once, then locked
        └── trs_test_<state>_NNN       clones of the template, grown on demand
              └── leased by one test at a time, reset when returned
```

Each test owns a real Postgres database for its duration. Nothing is shared, so tests can assert over whole
tables — counts, "list everything", search ordering — and run concurrently without seeing each other's rows.
The implementation lives in
[`TestCommon/Database/`](../tests/TeachingRecordSystem.TestCommon/Database/README.md).

### A database per run

Used by `EndToEndTests` and `SupportUi.EndToEndTests`.

These drive real Kestrel servers over HTTP with Playwright, so requests arrive on threads that have no xUnit
test context and the ambient lookup cannot work. Both projects are serialised anyway
(`parallelizeTestCollections: false`), so the fixture takes a single lease for the whole run with
`TestDatabases.AcquireForRunAsync()` and overlays its connection string onto the host's configuration. That
still isolates the run from every other project, but not one test from the next.

`Cli.Tests` is a halfway case: its commands build their own host from `IConfiguration`, so they can't see the
ambient data source either — but each test still leases its own database and overlays that database's
connection string onto the configuration the command is handed.

### What this replaced

Previously every test shared the `trs` database managed by
[`DbHelper`](../tests/TeachingRecordSystem.TestCommon/DbHelper.cs), with nothing cleaned up between tests.
Tests had to generate collision-free data and could not assert over whole tables. Tests needing a clean slate
opted into `[ClearDbBeforeTest]`, which only works inside a `DisableParallelization` collection — trading
concurrency for isolation. Those attributes are gone.

## How the pooled design works

**Template.** On the first run for a given schema, the migrations are applied to a scratch database, reference
and seed data are inserted, and the result is renamed into place and marked `IS_TEMPLATE` with
`ALLOW_CONNECTIONS false`. Building under a scratch name means a run that dies mid-migration cannot leave a
half-built template behind for the next run to clone. The template is reused across runs: its name contains a
hash of the EF model, so a schema change produces a new template rather than a stale one.

**Pool.** Databases are created with `CREATE DATABASE ... TEMPLATE`, which in Postgres is close to a file
copy. They are created **on demand**, not up front, so running a single test creates a single database — this
is what keeps the edit/run/fix loop cheap. The pool grows to `ProcessorCount + 4` by default
(`TestDatabasePoolSize` overrides it); past that, tests wait for one to come back.

**Lease.** A test takes a database in `InitializeAsync` and returns it in `DisposeAsync`. On return the
database is reset: the tables are truncated, and tables holding seeded rows are refilled from a snapshot taken
when the template was built. A database left behind by a previous run is reused if its name still matches,
since truncating is much cheaper than cloning.

**Failures.** When a test fails, its database is kept with its data intact, dropped from the pool, and named
in the test output, so the exact state that broke can be opened with `psql`.

## Adding or migrating a test project

1. **Fixture** — initialise the pool and point the host at it:

   ```csharp
   await TestDatabases.InitializeAsync();   // instead of InitializeDbAsync()
   services.AddPooledTestDatabase();        // last, so it wins
   ```

   Fixtures deriving from `ServiceProviderFixture` get both for free.

2. **Startup seeding** — anything the host wrote to the database at start-up has to move into the template
   via `TestDatabases.AddTemplateSeed(key, seed)`. A startup task would otherwise only populate whichever
   database happened to be leased at the time. `SupportUi.Tests` seeds its admin user and its test route types
   this way. Add the table to `_seededTables` in `TestDatabaseTemplate` so a reset restores those rows.

3. **Test base** — derive from `PooledDatabaseTestBase`, or lease a database in `InitializeAsync` and dispose
   it afterwards. Projects with a web host should also push a per-test DI scope (see below).

4. **Remove the escape hatches** — delete `[ClearDbBeforeTest]` and the `DisableParallelization` collections.
   An exclusive database per test makes both unnecessary.

5. **Parallelism** — `parallelMode` in `xunit.runner.json` controls how much concurrency you get. It is worth
   measuring rather than assuming: `all` cut `Core.Tests` from 24s to 17s, but made no useful difference to
   `SupportUi.Tests`, which already has enough test classes to fill the CPU. See "Choosing parallelMode".

## Pitfalls

Each of these produced a green-looking design that was quietly wrong during the migration, and each is worth
checking for in new code.

**Ambient state set in `InitializeAsync` is invisible to the test.** A value assigned to an `AsyncLocal`
inside an async method does not flow back to its caller. `TestScopedServices` gets away with it only because
`Reset()` is called from a *constructor*, which shares an execution context with the test method. Leasing a
database is async, so `TestDatabaseScope` keys off `TestContext` instead — which xUnit flows correctly,
including into the `TestServer` request pipeline (with `PreserveExecutionContext = true`).

**Scoped services resolved from the root provider are cached for the whole run.** Tests routinely call
`HostFixture.Services.GetRequiredService<SomeService>()`. Resolving a *scoped* service that way caches it in
the root scope, so the first test to resolve a service holding a `TrsDbContext` pins every later test to that
first test's database. One shared database hid this completely. Fixtures now expose `TestServiceScope.Current`
in place of the root provider, giving each test its own scope.

**Do not register the pool's `NpgsqlDataSource` in DI.** The container disposes the services it hands out, so
a closing request scope would dispose a data source the pool still owns. `AddPooledTestDatabase` keeps the
DI-built `DbContextOptions<TrsDbContext>` — so `PublishEventsDbCommandInterceptor` and anything else that has
decorated it stay attached — and swaps only the connection.

**Process-wide caches of database content stop being valid.** `ReferenceDataCache` is a singleton. Once each
test has its own database, a test that adds reference data publishes ids that exist nowhere else, and a
concurrent test referencing one fails on a foreign key. `PooledReferenceDataCaches` keeps one cache per
database. Anything else caching rows process-wide needs the same treatment.

**Tables holding both seeded and test-created rows can be neither truncated nor preserved.** `users` and
`training_providers` are truncated on reset and refilled from a template snapshot. Preserving them lets one
test's writes leak into the next; truncating them loses the seed.

**Cache keys must cover everything that determines the result.** The pooled database name hashes the schema
*and* the reset statement; the cached table list is keyed on the model assembly *and* the table
classifications. Both of these caused real bugs when they covered only the schema — in one case the reset
silently did nothing while still reporting success.

**Tests that assert over a whole table see the seeded rows.** `SupportUi.Tests` seeds an admin user so that
every test has a current user to act as; tests enumerating all users call `DeleteSeededUsersAsync()` first,
which is the modern equivalent of what `[ClearDbBeforeTest]` did for them.

## Choosing parallelMode

`parallelMode` (xUnit 4) takes `none`, `collections` (the default) or `all`. Collection-level parallelism is
capped by the number of test classes, so it degrades badly when tests are concentrated in a few large classes
and makes no difference when there are already more classes than CPU threads. Measured on a 14-thread machine
with 300 equivalent tests:

| Layout | `all` | `collections` |
| --- | --- | --- |
| 1 class × 300 tests | 1.65s | 4.54s |
| 3 × 100 | 1.62s | 2.35s |
| 14 × 22 | 1.63s | 1.54s |
| 30 × 10 | 1.61s | 1.72s |

`all` is worth enabling when a project has fewer test classes than CPU threads, or a few very large ones. It
requires per-test state to be genuinely per-test, since tests in the same class then run concurrently.

## Results

Measured on a 14-thread machine, comparing each project against the same suite before migration.

| Project | Tests | Before | After |
| --- | --- | --- | --- |
| `SupportUi.Tests` | 4,036 | 53.7s | 41–47s |
| `Api.IntegrationTests` | 3,823 | 52.3s, 1 failure | 25–31s |
| `Core.Tests` | 1,154 | 48.6s, 2 failures | 17–18s |
| `Cli.Tests` | 35 | 35.3s | 2.0s |
| `Api.UnitTests` | 149 | 6.1s | 6.5s |
| `AuthorizeAccess.Tests` | 82 | 4.3–5.6s | 3.8–4.4s |
| `SupportUi.EndToEndTests` | 153 | 138.2s | 134–136s |
| `EndToEndTests` | 19 | 73.1s, 2 failures | 73.4s, same 2 failures |

The two `Core.Tests` failures were the long-standing `RefreshTrainingProvidersJobTests` ones; they fail
consistently on the shared database and pass once each test gets a clean `training_providers` table. The two
`EndToEndTests` failures are the known `OidcTests` ones and fail identically before and after.

`Cli.Tests` gains the most proportionally: it was one serialised collection paying for a shared-schema
rebuild, and is now 35 independent tests.

## Running tests in a worktree

Unchanged: set `UseTestContainers=true` and `TestContainersPostgresPort` to a free port.

`DbHelper` still owns the container and the `trs` database. Nothing runs its tests against `trs` any more, but
it is still what starts the testcontainer, and `just remove-tests-schema-cache` still applies to it. The
pooled templates and databases live on the same server.

## AGENTS.md note

The instructions in `AGENTS.md` about resetting the test database schema still apply to the shared `trs`
database that `DbHelper` manages. Pooled templates key themselves on a hash of the EF model, so they rebuild
on their own when the schema changes and need no manual cache clearing.
