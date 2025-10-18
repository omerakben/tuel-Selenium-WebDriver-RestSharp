# Contributing to TUEL Test Framework

Thank you for your interest in contributing to TUEL Test Framework! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)

## Code of Conduct

This project adheres to a code of conduct that we expect all contributors to follow. Please be respectful, inclusive, and constructive in all interactions.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Create a new branch for your feature or bugfix
4. Make your changes
5. Test your changes thoroughly
6. Submit a pull request

## Development Setup

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Chrome or Edge browser
- Git

### Setup Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/TUEL-TestFramework.git
   cd TUEL-TestFramework
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

## Contributing Guidelines

### Types of Contributions

We welcome several types of contributions:

- **Bug Fixes**: Fix existing issues
- **Feature Additions**: Add new functionality
- **Documentation**: Improve documentation
- **Test Coverage**: Add or improve tests
- **Performance Improvements**: Optimize existing code
- **Code Quality**: Refactor and improve code quality

### Before You Start

1. Check existing issues and pull requests to avoid duplication
2. For significant changes, open an issue first to discuss the approach
3. Ensure your changes align with the project's goals and architecture

## Pull Request Process

### Creating a Pull Request

1. **Branch Naming**: Use descriptive branch names (e.g., `feature/add-retry-mechanism`, `fix/thread-sleep-issue`)

2. **Commit Messages**: Write clear, descriptive commit messages:
   ```
   feat: add retry mechanism for API calls
   fix: replace Thread.Sleep with WebDriverWait
   docs: update README with setup instructions
   ```

3. **Pull Request Title**: Use a clear, descriptive title

4. **Pull Request Description**: Include:
   - What changes were made
   - Why the changes were necessary
   - How to test the changes
   - Any breaking changes
   - Screenshots (for UI changes)

### Pull Request Requirements

- [ ] Code follows the project's coding standards
- [ ] All tests pass
- [ ] New tests are added for new functionality
- [ ] Documentation is updated if necessary
- [ ] No breaking changes (or clearly documented)
- [ ] Code is properly formatted
- [ ] No sensitive information is included

## Issue Reporting

### Before Reporting an Issue

1. Check if the issue already exists
2. Try to reproduce the issue
3. Gather relevant information

### Issue Template

When reporting an issue, please include:

- **Description**: Clear description of the issue
- **Steps to Reproduce**: Detailed steps to reproduce the issue
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Environment**: OS, .NET version, browser version
- **Screenshots**: If applicable
- **Logs**: Relevant error messages or logs

## Coding Standards

### C# Coding Standards

- Follow Microsoft's C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Use async/await for asynchronous operations
- Prefer `var` for local variables when type is obvious
- Use nullable reference types appropriately

### Test Standards

- Write descriptive test method names
- Use the AAA pattern (Arrange, Act, Assert)
- One assertion per test when possible
- Use meaningful test data
- Clean up resources in test cleanup

### Code Organization

- Keep classes focused and single-purpose
- Use appropriate access modifiers
- Group related functionality together
- Follow the existing project structure

## Testing Requirements

### Test Coverage

- New features must include comprehensive tests
- Bug fixes must include tests that prevent regression
- Aim for high test coverage (>90%)

### Test Types

- **Unit Tests**: Test individual methods and classes
- **Integration Tests**: Test component interactions
- **API Tests**: Test API endpoints
- **UI Tests**: Test user interface functionality

### Test Naming Convention

```csharp
[TestMethod]
[Description("Verifies that the API returns a successful status code")]
public async Task GetProducts_Returns_Status_OK()
{
    // Test implementation
}
```

## Development Workflow

### Feature Development

1. Create a feature branch from `main`
2. Implement the feature with tests
3. Ensure all tests pass
4. Update documentation if needed
5. Submit a pull request

### Bug Fixes

1. Create a bugfix branch from `main`
2. Implement the fix with tests
3. Ensure all tests pass
4. Submit a pull request

### Documentation Updates

1. Create a documentation branch
2. Update relevant documentation
3. Ensure accuracy and clarity
4. Submit a pull request

## Code Review Process

### For Contributors

- Respond to review feedback promptly
- Make requested changes
- Ask questions if feedback is unclear
- Be open to suggestions and improvements

### For Reviewers

- Provide constructive feedback
- Focus on code quality and correctness
- Be respectful and helpful
- Approve when satisfied with changes

## Release Process

1. All changes are merged to `main`
2. Version numbers are updated
3. Release notes are prepared
4. A new release is created
5. Documentation is updated

## Getting Help

If you need help or have questions:

- Open an issue for bugs or feature requests
- Use GitHub Discussions for general questions
- Check existing documentation
- Review existing code for examples

## Recognition

Contributors will be recognized in:

- CONTRIBUTORS.md file
- Release notes
- Project documentation

Thank you for contributing to TUEL Test Framework! 🚀
