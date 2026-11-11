# AI assistance and decisions

## 1. What did AI generate, and what did I write or modify myself?

- **I defined the direction:** the plan, technology stack, ERD, architecture, folder structure, and test project requirements.
- **AI generated most of the implementation.** I used Claude for planning; DeepSeek, Gemini Flash, and Codex for implementation; and Open Design for design. I also used the Elephant & Goldfish Model (EGM) approach during planning. AI helped draft this documentation.
- **I reviewed and refined the result:** I worked through the code step by step, cleaned up duplication and outdated approaches, revised pagination validation, and directed the Unit of Work implementation to match the intended structure.
- **Login scope:** I kept login simple to provide the JWT required by the task, rather than building a full user management system.

- **Observability:** I planned a monitoring demo but did not complete the dashboard integration. I wired OpenTelemetry tracing and metrics into the backend with optional OTLP export, and added Serilog request logs with timing and status codes, keeping ASP.NET Core logs at Warning level. This prepares the API for a monitoring backend such as Grafana.

## 2. What security issues did I find, and how did I handle them?

- **Vulnerable packages:** I identified vulnerability warnings in the generated dependency choices. AutoMapper was upgraded to 15.1.3 and the OpenTelemetry OTLP exporter to 1.15.3. The follow-up NuGet audit, including transitive dependencies, and `npm audit` reported zero known vulnerabilities.
- **Weak security design:** I rejected the initial proposal as insufficient even for a demo and requested a better implementation. The current API validates JWT signatures, issuer, audience, and expiry, and protects write endpoints with authentication.
- **Unprotected write endpoints:** Some generated write endpoints, including POST actions, were initially public. I caught this during review and required JWT authentication for them; the current write endpoints use `[Authorize]`.
- **Exposed keys:** I challenged suggestions that would expose keys in committed or deployed files and required external configuration for real secrets. Public demo credentials and a demo signing key remain in the repository and must be replaced before deployment.

## 3. One thing the AI got wrong that I had to fix

- **The mistake:** Track status updates fetched the track again after it had already been loaded and updated.
- **Why it was wrong:** The extra query was unnecessary because the updated entity was already available in memory.
- **My correction:** I removed the redundant fetch and reused the updated entity for the response. During the same review, I applied my EF Core experience to use `AsSplitQuery()` for track detail queries with related data, reducing duplication in joined results.
