# Contributing to Task Board API

Thank you for your interest in contributing to Task Board API! This document provides guidelines and best practices for contributing to this project.

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on the issue, not the person
- Help others learn and grow

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/Test.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Test your changes thoroughly
6. Commit with clear messages
7. Push to your fork
8. Open a Pull Request

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- A code editor (VS Code, Visual Studio, or Rider recommended)

### Building and Running

```bash
cd TaskBoardApi
dotnet build
dotnet run
```

## Coding Standards

### F# Conventions

#### Naming Conventions

- **Types and Modules**: PascalCase
  ```fsharp
  type TaskItem = { ... }
  module TaskService = ...
  ```

- **Functions and Values**: camelCase
  ```fsharp
  let createTask dto = ...
  let mutable taskCount = 0
  ```

- **Discriminated Unions**: PascalCase for both type and cases
  ```fsharp
  type TaskStatus =
      | NotStarted
      | InProgress
      | Completed
  ```

#### Code Organization

- Place type definitions before functions that use them
- Group related functionality together
- Use modules to organize code logically
- Keep functions small and focused

#### Indentation

- Use 4 spaces for indentation (as specified in .editorconfig)
- Align pipeline operators and match expressions consistently

```fsharp
// Good
tasks
|> List.filter (fun t -> t.Status = InProgress)
|> List.sortBy (fun t -> t.CreatedAt)
|> List.map (fun t -> t.Title)

// Match expressions
match result with
| Some value -> processValue value
| None -> handleNone()
```

#### Documentation

- Add XML documentation comments for public APIs
- Include usage examples for complex functions
- Document non-obvious behavior

```fsharp
/// Creates a new task from the provided DTO
/// Returns the created task with generated ID and timestamps
member _.CreateTask(dto: CreateTaskDto) =
    // implementation
```

### Project Structure

- **Models**: Data structures and DTOs
- **Storage**: Data persistence and queries
- **Services**: Business logic
- **Controllers**: HTTP endpoints

### Error Handling

- Use appropriate HTTP status codes
- Return meaningful error messages
- Log errors for debugging

### Testing

Currently, the project doesn't have automated tests. When adding tests:
- Write unit tests for services and storage
- Use xUnit or NUnit as testing framework
- Aim for high code coverage
- Test edge cases and error scenarios

## Pull Request Process

1. **Update Documentation**: Update README.md if you change functionality
2. **Follow Coding Standards**: Ensure your code follows the conventions above
3. **Test Your Changes**: Manually test all affected endpoints
4. **Clear Commit Messages**: Use descriptive commit messages
   ```
   Add CSV export functionality
   
   - Implement CSV generation in TaskService
   - Add export endpoint to TaskController
   - Handle special characters and escaping
   ```
5. **Small, Focused PRs**: Keep changes focused on a single feature or fix
6. **Respond to Feedback**: Be open to suggestions and changes

## Feature Requests and Bug Reports

### Reporting Bugs

Include:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment details (.NET version, OS)
- Relevant logs or error messages

### Suggesting Features

Include:
- Clear description of the feature
- Use cases and benefits
- Potential implementation approach
- Any breaking changes

## Code Review Guidelines

When reviewing code:
- Check for correctness and logic errors
- Verify adherence to coding standards
- Look for security issues
- Suggest improvements constructively
- Approve if changes are satisfactory

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

## Questions?

Feel free to open an issue for questions or clarifications about contributing.
