# Agent Instructions

## Build, Test, and Format Commands

- **Build the solution**: Use `just build` to build the .NET solution.
- **Test changes**: Use `just test-changed` to test projects affected by changes from the main branch.
- **Format changes**: Use `just format-changed` to format uncommitted .tf or .cs files.

## Code Style and Testing Guidelines

### Testing Patterns

- Tests must follow the **arrange, act, assert** pattern.
- Use `Record.Exception()` followed by a separate `Assert...` statement instead of `Assert.Throws()`.

Example:
```csharp
// Arrange
var service = new MyService();

// Act
var exception = Record.Exception(() => service.DoSomething());

// Assert
Assert.NotNull(exception);
Assert.IsType<ArgumentException>(exception);
```

Assume that dependencies for running tests (a local postgres database, Playwright etc.) are already configured.

### Test databases

Every test leases a Postgres database of its own, cloned from a template that is built once per schema and seed
data and rebuilt automatically when either changes; see `docs/test-databases.md`. Nothing is shared between tests,
so there is no data to clear down and no schema cache to reset.

In a git worktree, set `UseTestContainers` to `true` so the tests use a postgres container. Worktrees and test
projects can share that container, and can run at the same time, so `TestContainersPostgresPort` is only needed if
something else is using the default port. When `UseTestContainers` is set the container's connection string
overrides any `ConnectionStrings:DefaultConnection` from user secrets or the environment, so don't set
`ConnectionStrings__DefaultConnection` as well.

A failing test's database is kept and named in the test output so it can be inspected with `psql`. To drop these,
and templates for old schemas, run:

```shell
just drop-test-databases
```

### Boolean Expressions

- Prefer `!boolean-expression` over `boolean-expression == false`.
- Prefer `boolean-expression` over `boolean-expression == true`.

### Null Checks

- Prefer `is null` over `== null`.
- Prefer `is not null` over `!= null`.

### Code Formatting

- Follow the rules defined in the `.editorconfig` file.
- Follow the existing patterns in the codebase.
- C# files use 4-space indentation.
- File-scoped namespaces are required.
- Prefer braces for all code blocks.

## Completion Requirements

Before completing any work:

1. The solution must build without any errors or warnings using `just build`.
2. All tests affected by changes must pass using `just test-changed`.
3. All code changes must be formatted using `just format-changed`.
4. If there are any changes to emitted events, update docs/process-type-events.md accordingly.

These requirements ensure code quality and consistency across the codebase.
