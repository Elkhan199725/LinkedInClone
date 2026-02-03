# LinkedInClone API

A professional **LinkedIn-style social networking API** built with **.NET 10** and **ASP.NET Core**, following **Clean Architecture** principles.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4)](https://docs.microsoft.com/aspnet/core/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://docs.microsoft.com/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## ?? Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Authentication & Authorization](#-authentication--authorization)
- [API Endpoints](#-api-endpoints)
- [Testing with Swagger](#-testing-with-swagger)
- [Development Notes](#-development-notes)
- [Versioning](#-versioning)
- [Roadmap](#-roadmap)

---

## ? Features

- **JWT Authentication** – Secure Bearer token-based authentication
- **Role-Based Authorization** – Three-tier role system (User, Admin, SuperAdmin)
- **ASP.NET Core Identity** – Full identity management with GUID primary keys
- **Clean Architecture** – Separation of concerns across 4 layers
- **Swagger/OpenAPI** – Interactive API documentation with JWT support
- **Entity Framework Core** – Code-first database with SQL Server
- **Serilog Logging** – Structured logging with file and console sinks
- **Identity Seeding** – Automatic role creation and SuperAdmin seeding

---

## ?? Tech Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 10, ASP.NET Core |
| Database | SQL Server 2022, EF Core 10 |
| Authentication | ASP.NET Core Identity, JWT Bearer |
| Documentation | Swashbuckle (OpenAPI 3.0) |
| Logging | Serilog |
| Architecture | Clean Architecture |


---

## ?? Project Structure

```
LinkedInClone/
??? Api/                              # Presentation Layer
?   ??? Controllers/
?   ?   ??? AuthController.cs         # Registration & Login (MediatR)
?   ?   ??? ProfileController.cs      # Profile CRUD (MediatR)
?   ?   ??? MeController.cs           # Current user info
?   ?   ??? AdminController.cs        # Admin operations
?   ??? Middleware/
?   ?   ??? ExceptionHandlingMiddleware.cs
?   ??? Security/
?   ?   ??? JwtTokenGenerator.cs      # JWT token generation
?   ??? Program.cs                    # DI configuration
?
??? Application/                      # Business Logic Layer
?   ??? Auth/
?   ?   ??? Commands/                 # RegisterCommand, LoginCommand
?   ?   ??? Handlers/                 # Command handlers
?   ?   ??? Dtos/                     # Request/Response DTOs
?   ?   ??? Validators/               # FluentValidation
?   ??? Profiles/
?   ?   ??? Commands/                 # UpdateMyProfileCommand
?   ?   ??? Queries/                  # GetMyProfile, GetPublicProfile
?   ?   ??? Handlers/                 # Query/Command handlers
?   ?   ??? Dtos/                     # Profile DTOs
?   ?   ??? Validators/               # Profile validation
?   ?   ??? Mapping/                  # AutoMapper profiles
?   ??? Common/
?       ??? Interfaces/               # IBaseRepository, IUserProfileRepository
?       ??? Exceptions/               # Custom exceptions
?       ??? Behaviors/                # MediatR pipeline behaviors
?
??? Infrastructure/                   # Data Access Layer
?   ??? Persistence/
?   ?   ??? ApplicationDbContext.cs
?   ?   ??? IdentitySeeder.cs         # Role & admin seeding
?   ?   ??? Configurations/           # EF Fluent API configs
?   ?   ??? Repositories/             # Repository implementations
?   ??? Migrations/                   # EF Core migrations
?
??? Domain/                           # Core Domain Layer
?   ??? Entities/
?   ?   ??? AppUser.cs                # Identity-only user
?   ?   ??? UserProfile.cs            # Profile data (shared PK)
?   ??? Constants/
?       ??? AppRoles.cs               # Role definitions
?
??? README.md
??? ARCHITECTURE.md                   # Detailed architecture docs
```

> ?? **For detailed architecture documentation**, see **[ARCHITECTURE.md](ARCHITECTURE.md)**

### Layer Responsibilities

| Layer | Responsibility |
|-------|----------------|
| **Api** | HTTP handling, thin controllers (MediatR only), middleware |
| **Application** | CQRS commands/queries, handlers, DTOs, validators, interfaces |
| **Infrastructure** | EF Core, repositories, Identity, external services |
| **Domain** | Entities, constants, domain logic (zero dependencies) |

---

## ?? Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2022](https://www.microsoft.com/sql-server) (or Docker)
- IDE: Visual Studio 2022+, VS Code, or JetBrains Rider

### 1. Clone the Repository

```bash
git clone https://github.com/Elkhan199725/LinkedInClone.git
cd LinkedInClone
```

### 2. Configure the Database

**Option A: Using Docker (Recommended)**

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Password123" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**Option B: Local SQL Server**

Ensure SQL Server is running on `localhost,1433`.

### 3. Configure Application Settings

Create `Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LinkedInCloneDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;"
  },
  "SeedAdmin": {
    "Email": "superadmin@local.test",
    "Password": "SuperAdmin123!"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32Characters!"
  }
}
```

> ?? **Security Note**: `appsettings.Development.json` is git-ignored and should never be committed.

### 4. Apply Database Migrations

```bash
cd Api
dotnet ef database update --project ../Infrastructure
```

### 5. Run the Application

```bash
dotnet run --project Api
```

The API will start at:
- **HTTPS**: `https://localhost:7xxx`
- **Swagger UI**: `https://localhost:7xxx/swagger`

---

## ?? Authentication & Authorization

### Authentication Flow

```
???????????????     POST /api/auth/register     ???????????????
?   Client    ? ??????????????????????????????  ?     API     ?
?             ?                                  ?             ?
?             ?  ??????????????????????????????  ?  Returns:   ?
?             ?     { userId, email, token }     ?  JWT Token  ?
???????????????                                  ???????????????
       ?
       ?  Include in subsequent requests:
       ?  Authorization: Bearer <token>
       ?
???????????????     GET /api/me (Authorized)    ???????????????
?   Client    ? ??????????????????????????????  ?     API     ?
?  + JWT      ?                                  ?  Validates  ?
?             ?  ??????????????????????????????  ?    Token    ?
?             ?     { userId, email, roles }     ?             ?
???????????????                                  ???????????????
```

### Role Hierarchy

| Role | Permissions |
|------|-------------|
| **User** | Default role for registered users |
| **Admin** | User management capabilities |
| **SuperAdmin** | Full system access, can assign roles |

### JWT Token Structure

```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "jti": "unique-token-id",
  "iat": 1234567890,
  "role": ["User"],
  "exp": 1234571490
}
```

---

## ?? API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register new user | No |
| `POST` | `/api/auth/login` | Login and get JWT | No |

### User

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/me` | Get current user info | Yes (any role) |

### Admin

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/admin/set-role` | Assign role to user | Yes (SuperAdmin) |

---

## ?? Testing with Swagger

1. **Open Swagger UI**  
   Navigate to `https://localhost:7xxx/swagger`

2. **Register or Login**  
   Use `/api/auth/register` or `/api/auth/login` to obtain a JWT token.

3. **Authorize in Swagger**  
   - Click the **"Authorize"** button (??) in the top-right
   - Enter your token (without "Bearer " prefix)
   - Click **"Authorize"**

4. **Test Protected Endpoints**  
   - Try `GET /api/me` – should return your user info
   - Try `POST /api/admin/set-role` (requires SuperAdmin role)

### Example: Login Request

```json
POST /api/auth/login
{
  "email": "superadmin@local.test",
  "password": "SuperAdmin123!"
}
```

### Example: Response

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "superadmin@local.test",
  "roles": ["SuperAdmin"],
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## ?? Development Notes

### Entity Framework Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Api

# Apply migrations
dotnet ef database update --project Infrastructure --startup-project Api

# Remove last migration (if not applied)
dotnet ef migrations remove --project Infrastructure --startup-project Api
```

### Configuration & Secrets

| Setting | Location | Notes |
|---------|----------|-------|
| Connection String | `appsettings.Development.json` | Git-ignored |
| JWT Key | `appsettings.Development.json` | Min 32 characters |
| Seed Admin | `appsettings.Development.json` | Optional |

**Production**: Use environment variables or Azure Key Vault:

```bash
export ConnectionStrings__DefaultConnection="Server=..."
export Jwt__Key="YourProductionSecretKey..."
```

### Logging

Logs are written to:
- **Console**: Real-time output
- **File**: `Api/Logs/log-{date}.txt` (14-day retention)

---

## ?? Versioning

### Current Version: `v0.1.0`

**Release Date**: February 2026

#### What's Included

- ? ASP.NET Core Identity with GUID keys
- ? JWT Bearer authentication
- ? Role-based authorization (User, Admin, SuperAdmin)
- ? User registration and login
- ? Identity seeding (roles + SuperAdmin)
- ? Swagger UI with JWT support
- ? EF Core with SQL Server
- ? Serilog structured logging
- ? Clean Architecture foundation

#### Semantic Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

---

## ?? Roadmap

### v0.2.0 – User Profiles
- [ ] Profile CRUD operations
- [ ] Profile photo upload
- [ ] User search

### v0.3.0 – Connections
- [ ] Connection requests
- [ ] Accept/reject connections
- [ ] Connection list

### v0.4.0 – Posts & Feed
- [ ] Create/edit/delete posts
- [ ] News feed
- [ ] Like and comment

### v1.0.0 – Production Ready
- [ ] Refresh tokens
- [ ] Email verification
- [ ] Password reset
- [ ] Rate limiting
- [ ] Docker Compose setup
- [ ] CI/CD pipeline

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

<p align="center">
  Built with ?? using .NET 10
</p>
