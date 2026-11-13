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

Assume that other test dependencies, such as Playwright, are already installed.

### Test databases

Tests run against a Postgres container that's started automatically, so Docker must be running; no configuration is needed.
Each test process gets its own database, so test projects and git worktrees can run at the same time.
Use `just test-project Core.Tests` to run a whole project faster, across several processes.
See [docs/test-databases.md](docs/test-databases.md).

### Boolean Expressions

- Prefer `!boolean-expression` over `boolean-expression == false`.
- Prefer `boolean-expression` over `boolean-expression == true`.

### Null Checks

- Prefer `is null` over `== null`.
- Prefer `is not null` over `!= null`.

### Code Formatting

- Follow the rules defined in the `.editorconfig` file.
- C# files use 4-space indentation.
- File-scoped namespaces are required.
- Prefer braces for all code blocks.

## Completion Requirements

Before completing any work:

1. The solution must build without any errors or warnings using `just build`.
2. All tests affected by changes must pass using `just test-changed`.
3. All code changes must be formatted using `just format-changed`.
4. If there are any changes to emitted events, update docs/process-type-events.md accordingly.
