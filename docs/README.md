# EnterpriseTestFramework Documentation

Welcome to the EnterpriseTestFramework documentation! This comprehensive guide will help you understand, set up, and use the framework effectively.

## Table of Contents

- [Getting Started](getting-started.md)
- [Architecture Overview](architecture.md)
- [API Testing Guide](api-testing.md)
- [UI Testing Guide](ui-testing.md)
- [Configuration](configuration.md)
- [Authentication](authentication.md)
- [Page Object Model](page-object-model.md)
- [Best Practices](best-practices.md)
- [Troubleshooting](troubleshooting.md)
- [Examples](examples/)
- [API Reference](api-reference.md)

## Quick Start

1. **Prerequisites**: .NET 8.0 SDK, Visual Studio 2022 or VS Code, Chrome/Edge browser
2. **Setup**: Clone the repository and restore dependencies
3. **Configuration**: Copy and configure the runsettings file
4. **Run Tests**: Execute `dotnet test` to run the test suite

## Key Features

- **Modern .NET 8.0**: Latest C# features and performance improvements
- **Comprehensive Testing**: API and UI test coverage
- **Page Object Model**: Maintainable UI test architecture
- **Authentication**: Multi-flow Azure AD integration
- **Configuration Management**: Environment-specific settings
- **Retry Mechanisms**: Robust error handling and recovery
- **Performance Optimized**: Efficient wait strategies
- **Docker Ready**: Containerized setup for easy deployment

## Architecture

The framework follows a layered architecture:

```
┌─────────────────────────────────────┐
│           Test Classes              │
├─────────────────────────────────────┤
│        Page Object Models           │
├─────────────────────────────────────┤
│         Support Classes             │
├─────────────────────────────────────┤
│        Configuration Layer          │
├─────────────────────────────────────┤
│         Authentication              │
└─────────────────────────────────────┘
```

## Getting Help

- 📖 Check the [documentation](docs/)
- 🐛 Report issues on [GitHub Issues](https://github.com/yourusername/EnterpriseTestFramework/issues)
- 💬 Join discussions on [GitHub Discussions](https://github.com/yourusername/EnterpriseTestFramework/discussions)
- 📧 Contact: [your-email@example.com](mailto:your-email@example.com)

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with ❤️ by [Omer Akben](https://omerakben.com) - Enterprise SDET Architect**
