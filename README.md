# Transit to Fully Open-Source TUEL Test Framework

Modern C# test automation template, evolved from a legacy Letter of Credit solution into a vendor-neutral, community-ready baseline.

A comprehensive, enterprise-grade test automation framework built with .NET 8.0, Selenium WebDriver, and RestSharp. This framework demonstrates modern SDET architecture patterns and serves as a plug-and-play template for API and UI testing.

## 🚀 Features

- **Modern .NET 8.0** with latest C# features
- **Comprehensive Testing**: API + UI test coverage
- **Page Object Model**: Maintainable UI test architecture
- **Authentication**: Multi-flow Azure AD integration
- **Configuration Management**: Environment-specific settings
- **Retry Mechanisms**: Robust error handling and recovery
- **Performance Optimized**: Efficient wait strategies
- **Docker Ready**: Containerized setup for easy deployment

## 🏗️ Architecture

### Test Layers
- **API Tests**: RESTful API testing with RestSharp
- **UI Tests**: Selenium WebDriver with Page Object Model
- **Authentication**: Microsoft Entra ID integration
- **Configuration**: Centralized environment management

### Domain Model
- **Products**: Core business entities
- **Orders**: Transaction management
- **Customers**: User/client management
- **Dashboard**: Business intelligence
- **Templates**: Reusable configurations
- **Pricing**: Fee and cost management

## 🛠️ Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Chrome or Edge browser
- Docker (optional)

### Setup
1. Clone the repository.
2. Change into the test project directory: `cd loc.test`.
3. Copy `TUEL.TestFramework.runsettings.example` to `TUEL.TestFramework.runsettings`.
4. Update the new file with your environment-specific values (or create `TUEL.TestFramework.local.runsettings` for machine-specific overrides).
5. Review the core expectations in [`docs/testing-guidelines.md`](docs/testing-guidelines.md).
6. Run tests:
   ```bash
   dotnet test TUEL.TestFramework.sln --settings TUEL.TestFramework.runsettings
   ```

### Docker Setup
```bash
docker-compose up -d
docker exec -it tuel-test-framework dotnet test
```

## 📁 Project Structure

```
.
├── docs/
├── loc.test/
│   ├── API/
│   ├── Configuration/
│   ├── Logging/
│   ├── Web/
│   ├── GlobalUsings.cs
│   ├── InitializeTestAssembly.cs
│   ├── TUEL.TestFramework.sln
│   ├── loc.test.csproj
│   ├── TUEL.TestFramework.runsettings.example
│   └── README.md
├── logs/
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 🔧 Configuration

### Environment Variables
- `ENV`: Environment name (dev, staging, prod)
- `BaseURL`: Application base URL
- `BaseurlAPI`: API base URL
- `DefaultTimeoutSeconds`: Default timeout for operations

### Authentication
- **Azure AD**: Client credentials flow
- **ROPC**: Resource owner password credentials
- **Local JWT**: Development/testing tokens

## 🧪 Test Categories

### API Tests
- Product management endpoints
- Order processing APIs
- Customer data APIs
- Authentication flows

### UI Tests
- Dashboard functionality
- Product management
- Order workflows
- Customer management
- Template configuration

## 📊 Reporting

- **Test Results**: Detailed test execution reports
- **Performance Metrics**: Execution time tracking
- **Screenshots**: Failure capture and analysis
- **Logs**: Comprehensive logging for debugging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎯 Use Cases

- **Enterprise Applications**: CRM, ERP, e-commerce platforms
- **API Testing**: RESTful services and microservices
- **UI Automation**: Web application testing
- **Regression Testing**: Continuous integration pipelines
- **Performance Testing**: Load and stress testing

## 🤝 Community

- Read the [`VISION.md`](VISION.md) to understand the long-term roadmap and decision-making principles.
- All contributors must follow the [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
- Contribution steps, coding standards, and review expectations live in [`CONTRIBUTING.md`](CONTRIBUTING.md).
- Testing expectations are detailed in [`docs/testing-guidelines.md`](docs/testing-guidelines.md).

## 🆘 Support & Security

- Need help? Check [`SUPPORT.md`](SUPPORT.md) for the right channel.
- Report security issues privately via the process in [`SECURITY.md`](SECURITY.md). Do not open public issues for vulnerabilities.

## 🔗 Links

- [Documentation](docs/)
- [Examples](samples/)
- [Issues](https://github.com/omerakben/transit-to-open-source-tuel/issues)
- [Portfolio](https://omerakben.com)

## 📈 Metrics

- **Test Coverage**: 95%+ code coverage
- **Execution Time**: < 5 minutes for full suite
- **Reliability**: 99%+ test stability
- **Maintainability**: Clean architecture patterns

---

Built with ❤️ by [Omer “Ozzy” Akben](https://omerakben.com) — Full-Stack Developer • AI Engineer • SDET • me@omerakben.com • (267) 512-4566
