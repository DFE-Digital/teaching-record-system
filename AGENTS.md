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

Each test project uses a database of its own, named `trs_<hash of the repository root>_<project>`. Whatever connection string is
configured — from user secrets, the environment, or a testcontainer — supplies the server and the credentials only; the database name
always comes from the tests. That means test projects can run at the same time, and worktrees can share a postgres server, without
clearing each other's data down mid-run. Nothing needs setting up per worktree.

Set `UseTestContainers` to `true` to run against a postgres container instead of a configured server. `TestContainersPostgresPort`
overrides the port it binds to, which is only needed if something else is already using the default.

### Resetting the database schema and data

The version of the schema a test database was built from is recorded as a comment on the database itself, so the schema and data are
recreated automatically whenever they don't match the current model and seed data — including when the database has been dropped or
replaced behind the tests' back. To force a rebuild, drop the project's test database and run the tests again.

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
