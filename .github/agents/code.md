# Copilot Coding Agent Instructions

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

These requirements ensure code quality and consistency across the codebase.
