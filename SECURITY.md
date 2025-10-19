# Security Policy

## Supported Versions

The Transit to Fully Open-Source TUEL project follows semantic versioning. The latest released minor version receives security updates. Older versions may receive fixes on a best-effort basis depending on severity and available maintainers.

| Version          | Supported            |
| ---------------- | -------------------- |
| main             | ✅ actively supported |
| < latest release | ⚠️ best effort        |

## Reporting a Vulnerability

We take security issues seriously and appreciate your help in keeping the framework safe for everyone.

1. **Do not create a public issue** for suspected vulnerabilities.
2. Email the maintainers at [me@omerakben.com](mailto:me@omerakben.com) with the following details:
   - Detailed description of the vulnerability
   - Steps to reproduce or proof-of-concept code
   - Expected vs. actual behavior
   - Any potential impact you have identified
3. You will receive an acknowledgement within **72 hours**. We will work with you to assess the report and schedule a fix.

## Disclosure Process

- We will confirm the issue, determine severity, and identify mitigation steps.
- A fix will be prepared and tested privately.
- Once a fix is released, we will document the vulnerability in the release notes and credit the reporter (unless anonymity is requested).
- Coordinated disclosure timelines may vary depending on complexity; we aim to release fixes within **30 days** for high severity issues.

## Security Best Practices for Contributors

- Never commit secrets, API keys, or credentials. Use environment variables or secret stores.
- Follow the repository's testing guidelines; ensure new code paths are covered by automated tests.
- Prefer using secure defaults (HTTPS endpoints, secure cookies, parameterized queries).
- Review third-party dependencies for known vulnerabilities and update them regularly.
