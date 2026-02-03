# LinkedInClone Backend — Architecture Documentation

> **Project**: LinkedInClone  
> **Type**: RESTful Web API  
> **Framework**: ASP.NET Core (.NET 10)  
> **Architecture**: Clean Architecture + CQRS  
> **Status**: Foundation Complete — Ready for Feature Expansion

---

## Table of Contents

1. [Overview](#1-overview)
2. [Solution Structure](#2-solution-structure)
3. [Domain Layer](#3-domain-layer)
4. [Application Layer](#4-application-layer)
5. [Infrastructure Layer](#5-infrastructure-layer)
6. [API Layer](#6-api-layer)
7. [Authentication Module](#7-authentication-module)
8. [Profile Module](#8-profile-module)
9. [Repository Strategy](#9-repository-strategy)
10. [Cross-Cutting Concerns](#10-cross-cutting-concerns)
11. [Database Design](#11-database-design)
12. [Current State & Roadmap](#12-current-state--roadmap)
13. [Architectural Rules](#13-architectural-rules)

---

## 1. Overview

### What is LinkedInClone?

LinkedInClone is a professional networking platform backend API, modeled after LinkedIn. It provides authentication, user profiles, and is architected to support future features like posts, connections, and feeds.

### Tech Stack

| Category | Technology |
|----------|------------|
| **Framework** | ASP.NET Core 10 |
| **Language** | C# 14 |
| **Database** | SQL Server + EF Core 10 |
| **Authentication** | ASP.NET Core Identity + JWT Bearer |
| **Architecture** | Clean Architecture |
| **Patterns** | CQRS, Repository, Mediator |
| **Libraries** | MediatR, FluentValidation, AutoMapper, Serilog |
| **API Docs** | Swagger / OpenAPI |

### Why Clean Architecture?

Clean Architecture ensures:
- **Separation of Concerns**: Each layer has a single responsibility
- **Testability**: Business logic can be tested without infrastructure
- **Flexibility**: Infrastructure can be swapped without affecting business rules
- **Maintainability**: Changes in one layer don't cascade to others

---

## 2. Solution Structure

```
LinkedInClone/
??? Api/                    # Presentation Layer (Controllers, Middleware)
??? Application/            # Business Logic (CQRS, Handlers, Validators)
??? Domain/                 # Core Entities (No dependencies)
??? Infrastructure/         # Data Access, External Services
??? LinkedInClone.sln
```

### Layer Dependencies

```
    ???????????????
    ?     Api     ?  ??? Depends on ???? Application, Infrastructure
    ???????????????
           ?
           ?
    ???????????????
    ? Application ?  ??? Depends on ???? Domain
    ???????????????
           ?
           ?
    ???????????????
    ?   Domain    ?  ??? No dependencies
    ???????????????
           ?
           ?
    ???????????????
    ?Infrastructure? ??? Depends on ???? Application, Domain
    ???????????????
```

### Layer Responsibilities

| Layer | Responsibility | Must NOT |
|-------|----------------|----------|
| **Domain** | Entities, constants, domain logic | Reference any other project |
| **Application** | Commands, queries, handlers, DTOs, validators, interfaces | Reference Infrastructure or Api |
| **Infrastructure** | EF Core, repositories, Identity, external services | Contain business logic |
| **Api** | Controllers, middleware, DI configuration | Contain business logic |

---

## 3. Domain Layer

The Domain layer contains the core business entities. It has **zero external dependencies**.

### Entities

#### AppUser (Identity-Only)

```csharp
public class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Design Decision**: `AppUser` contains **only identity fields**. All profile data is stored in `UserProfile`. This separation allows:
- Identity concerns (authentication) to remain isolated
- Profile data to be managed independently
- Future support for multiple profile types if needed

#### UserProfile (Profile Data)

```csharp
public class UserProfile
{
    public Guid AppUserId { get; set; }    // PK + FK (Shared Primary Key)
    public AppUser AppUser { get; set; }   // Navigation property
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Headline { get; set; }
    public string? About { get; set; }
    public string? Location { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public bool IsPublic { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Key Point**: `UserProfile` uses a **Shared Primary Key** pattern. See [Section 8](#8-profile-module) for details.

### Constants

```csharp
public static class AppRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";
    
    public static readonly string[] All = [User, Admin, SuperAdmin];
}
```

---

## 4. Application Layer

The Application layer contains all business logic following the **CQRS pattern** (Command Query Responsibility Segregation).

### Structure

```
Application/
??? Auth/
?   ??? Commands/          # RegisterCommand, LoginCommand
?   ??? Dtos/              # RegisterRequest, LoginRequest, AuthResponse
?   ??? Handlers/          # RegisterCommandHandler, LoginCommandHandler
?   ??? Validators/        # FluentValidation validators
??? Profiles/
?   ??? Commands/          # UpdateMyProfileCommand
?   ??? Queries/           # GetMyProfileQuery, GetPublicProfileQuery
?   ??? Dtos/              # MyProfileResponse, PublicProfileResponse
?   ??? Handlers/          # Query and command handlers
?   ??? Validators/        # Profile validation rules
?   ??? Mapping/           # AutoMapper profiles
??? Common/
?   ??? Behaviors/         # MediatR pipeline behaviors
?   ??? Exceptions/        # Custom exception types
?   ??? Interfaces/        # Repository interfaces, IJwtTokenService
??? DependencyInjection.cs
```

### CQRS Pattern

**Commands** modify state:
```csharp
public sealed record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
public sealed record UpdateMyProfileCommand(Guid AppUserId, UpdateMyProfileRequest Request) : IRequest<MyProfileResponse>;
```

**Queries** read state:
```csharp
public sealed record GetMyProfileQuery(Guid AppUserId) : IRequest<MyProfileResponse>;
public sealed record GetPublicProfileQuery(Guid AppUserId) : IRequest<PublicProfileResponse>;
```

### Interface Definitions

Interfaces are defined in Application (not Infrastructure) to maintain the dependency rule:

```csharp
// Repository interfaces
public interface IBaseRepository<T> where T : class { ... }
public interface IUserProfileRepository : IBaseRepository<UserProfile> { }

// Service interfaces
public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string? userName, IEnumerable<string> roles);
}
```

---

## 5. Infrastructure Layer

The Infrastructure layer implements interfaces defined in Application and handles all external concerns.

### Structure

```
Infrastructure/
??? Persistence/
?   ??? ApplicationDbContext.cs
?   ??? ApplicationDbContextFactory.cs    # For EF migrations
?   ??? IdentitySeeder.cs                 # Seeds roles and admin
?   ??? Configurations/                   # EF Fluent API configs
?   ?   ??? AppUserConfiguration.cs
?   ?   ??? UserProfileConfiguration.cs
?   ??? Repositories/
?       ??? BaseRepository.cs
?       ??? UserProfileRepository.cs
??? Migrations/                           # EF Core migrations
??? DependencyInjection.cs
```

### DbContext

```csharp
public sealed class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

---

## 6. API Layer

The API layer contains thin controllers that delegate all work to MediatR.

### Structure

```
Api/
??? Controllers/
?   ??? AuthController.cs      # Register, Login
?   ??? ProfileController.cs   # Profile CRUD
?   ??? AdminController.cs     # Admin operations
?   ??? MeController.cs        # Current user info
??? Middleware/
?   ??? ExceptionHandlingMiddleware.cs
??? Security/
?   ??? JwtTokenGenerator.cs   # Implements IJwtTokenService
??? Program.cs                 # DI configuration
```

### Controller Design Principles

Controllers are **thin** — they only:
1. Receive HTTP requests
2. Create MediatR commands/queries
3. Send to mediator
4. Return results

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterCommand(request), cancellationToken);
        return Ok(result);
    }
}
```

**Controllers MUST NOT**:
- Contain business logic
- Directly use repositories
- Handle exceptions (middleware does this)
- Perform validation (FluentValidation pipeline does this)

---

## 7. Authentication Module

### Design Decisions

The Auth module uses **MediatR** but does **not** use a repository for `AppUser` because:

1. **ASP.NET Core Identity provides UserManager/SignInManager** — These are highly optimized, battle-tested services for identity operations
2. **AppUser is managed by Identity** — Using a custom repository would bypass Identity's security features (password hashing, lockout, token generation)
3. **Separation of concerns** — Auth deals with identity; repositories deal with domain aggregates

### Register Flow

```
???????????????     ???????????????????     ??????????????????????????
? Controller  ??????? RegisterCommand ??????? RegisterCommandHandler ?
???????????????     ???????????????????     ??????????????????????????
                                                       ?
                    ???????????????????????????????????????????????????????????????????????
                    ?                                  ?                                   ?
                    ?                                  ?                                   ?
            ?????????????????                 ?????????????????                  ???????????????
            ? FluentValidation ?              ? UserManager   ?                  ? JWT Service ?
            ? (Pipeline)       ?              ? .CreateAsync  ?                  ? .Generate   ?
            ?????????????????                 ?????????????????                  ???????????????
                                                       ?
                                                       ?
                                              ?????????????????????
                                              ? UserProfileRepo   ?
                                              ? .AddAsync         ?
                                              ?????????????????????
```

**Step-by-step**:
1. Controller receives `RegisterRequest`
2. Creates `RegisterCommand` and sends via MediatR
3. FluentValidation pipeline validates request
4. `RegisterCommandHandler` executes:
   - Checks if email exists (via `UserManager`)
   - Creates `AppUser` (via `UserManager.CreateAsync`)
   - Creates `UserProfile` (via `IUserProfileRepository`)
   - Assigns default "User" role
   - Generates JWT token
5. Returns `AuthResponse` with token

### Login Flow

```
???????????????     ????????????????     ???????????????????????
? Controller  ??????? LoginCommand ??????? LoginCommandHandler ?
???????????????     ????????????????     ???????????????????????
                                                    ?
                    ?????????????????????????????????????????????????????????????????
                    ?                               ?                                ?
                    ?                               ?                                ?
            ?????????????????             ???????????????????              ???????????????
            ? FluentValidation ?          ? SignInManager   ?              ? JWT Service ?
            ? (Pipeline)       ?          ? .CheckPassword  ?              ? .Generate   ?
            ?????????????????             ???????????????????              ???????????????
```

**Step-by-step**:
1. Controller receives `LoginRequest`
2. Creates `LoginCommand` and sends via MediatR
3. FluentValidation pipeline validates request
4. `LoginCommandHandler` executes:
   - Finds user by email
   - Verifies password with lockout support
   - Gets user's roles
   - Generates JWT token
5. Returns `AuthResponse` with token

---

## 8. Profile Module

### UserProfile Aggregate

`UserProfile` is a **separate aggregate** from `AppUser`. This design allows:
- Profile operations without touching Identity tables
- Potential future support for multiple profile types
- Clean separation between authentication and profile concerns

### Shared Primary Key Pattern

```
???????????????????????????????????????????????????????????????????
?                        AspNetUsers                               ?
?  ????????????????????????????????????????????????????????????   ?
?  ? Id (PK)  ? Email          ? PasswordHash ? CreatedAt     ?   ?
?  ? GUID     ? user@email.com ? ************ ? 2024-01-01    ?   ?
?  ????????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????????
                              ?
                              ? 1:1 Relationship
                              ? (Same GUID used)
                              ?
???????????????????????????????????????????????????????????????????
?                        UserProfiles                              ?
?  ????????????????????????????????????????????????????????????   ?
?  ? AppUserId (PK+FK) ? FirstName ? LastName ? IsPublic      ?   ?
?  ? GUID (same!)      ? John      ? Doe      ? true          ?   ?
?  ????????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????????
```

**Why Shared PK?**
1. **Simplicity**: No separate `Id` column needed on UserProfile
2. **Performance**: Direct join on PK = FK is extremely fast
3. **Data integrity**: Impossible to have orphaned profiles
4. **API clarity**: `GET /api/profile/{userId}` uses the same ID everywhere

**EF Core Configuration**:
```csharp
public void Configure(EntityTypeBuilder<UserProfile> builder)
{
    builder.HasKey(x => x.AppUserId);  // PK is AppUserId
    
    builder.HasOne(x => x.AppUser)
        .WithOne()
        .HasForeignKey<UserProfile>(x => x.AppUserId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

### Public vs Private Profiles

```csharp
public bool IsPublic { get; set; } = true;
```

- **Public (`IsPublic = true`)**: Anyone can view via `GET /api/profile/{userId}`
- **Private (`IsPublic = false`)**: Only the owner can view via `GET /api/profile/me`

The handler enforces this:
```csharp
if (!profile.IsPublic)
    throw new NotFoundException($"Profile for user '{request.AppUserId}' is not public.");
```

### Profile Endpoints

| Endpoint | Auth | Description |
|----------|------|-------------|
| `GET /api/profile/me` | Required | Get own profile |
| `PUT /api/profile/me` | Required | Update/create own profile (UPSERT) |
| `GET /api/profile/{userId}` | None | Get public profile |

---

## 9. Repository Strategy

### Why Entity-Specific Repositories?

The project follows a **teacher-mandated convention**: every domain entity must have its own repository interface and implementation, even if it only wraps `BaseRepository`.

```
Application/Common/Interfaces/
??? IBaseRepository<T>           # Generic operations
??? IUserProfileRepository       # Marker interface

Infrastructure/Persistence/Repositories/
??? BaseRepository<T>            # Generic implementation
??? UserProfileRepository        # Specific implementation
```

### BaseRepository

```csharp
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

### Why Auth Has No Repository

| Entity | Uses Repository? | Reason |
|--------|------------------|--------|
| `UserProfile` | ? Yes | Domain aggregate, managed by application |
| `AppUser` | ? No | Managed by ASP.NET Core Identity |

`AppUser` operations go through `UserManager<AppUser>` and `SignInManager<AppUser>` because:
- Identity handles password hashing, validation, lockout
- Identity manages security stamps, tokens
- Bypassing Identity would create security vulnerabilities

---

## 10. Cross-Cutting Concerns

### MediatR Pipeline

```
Request ??? Validation Behavior ??? Handler ??? Response
                   ?
                   ?
            FluentValidation
            (throws if invalid)
```

```csharp
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Run all validators
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
            
        if (failures.Any())
            throw new ValidationException(failures);
            
        return await next();
    }
}
```

### FluentValidation

Validators are automatically discovered and registered:

```csharp
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain uppercase")
            .Matches("[0-9]").WithMessage("Must contain digit");
    }
}
```

### AutoMapper

Used for entity ? DTO mapping in Profile module:

```csharp
public sealed class ProfileMappingProfile : Profile
{
    public ProfileMappingProfile()
    {
        CreateMap<UserProfile, MyProfileResponse>();
        CreateMap<UserProfile, PublicProfileResponse>();
        CreateMap<UpdateMyProfileRequest, UserProfile>()
            .ForMember(dest => dest.AppUserId, opt => opt.Ignore());
    }
}
```

### Global Exception Handling

All exceptions are caught by middleware and converted to consistent JSON responses:

| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| `NotFoundException` | 404 | NOT_FOUND |
| `AuthenticationException` | 401 | AUTHENTICATION_FAILED |
| `RegistrationException` | 400 | REGISTRATION_FAILED |
| `ValidationException` | 400 | VALIDATION_ERROR |
| `ForbiddenException` | 403 | FORBIDDEN |
| `UnauthorizedAccessException` | 401 | UNAUTHORIZED |
| (any other) | 500 | INTERNAL_ERROR |

**Response Format**:
```json
{
  "traceId": "abc123",
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": [
      { "field": "Email", "message": "Email is required." }
    ]
  }
}
```

### Authentication & Authorization

- **JWT Bearer tokens** are used for API authentication
- Tokens contain: `sub` (userId), `email`, `role` claims
- Token expiration is configurable (default: 60 minutes)
- Role-based authorization via `[Authorize(Roles = "...")]`

---

## 11. Database Design

### Schema Overview

```
???????????????????       ???????????????????
?   AspNetUsers   ?       ?   UserProfiles  ?
???????????????????       ???????????????????
? Id (PK)         ????????? AppUserId (PK)  ?
? Email           ?   1:1 ? FirstName       ?
? UserName        ?       ? LastName        ?
? PasswordHash    ?       ? Headline        ?
? CreatedAt       ?       ? About           ?
? UpdatedAt       ?       ? Location        ?
? ...Identity...  ?       ? IsPublic        ?
???????????????????       ? CreatedAt       ?
                          ? UpdatedAt       ?
                          ???????????????????

???????????????????       ???????????????????
?  AspNetRoles    ?       ?AspNetUserRoles  ?
???????????????????       ???????????????????
? Id (PK)         ????????? RoleId (FK)     ?
? Name            ?       ? UserId (FK)     ?
? NormalizedName  ?       ???????????????????
???????????????????               ?
                                  ?
                          ?????????????????
                          ?  AspNetUsers  ?
                          ?????????????????
```

### Migration History

| Migration | Purpose |
|-----------|---------|
| `InitialIdentity` | Create Identity tables |
| `AddUserProfiles` | Create UserProfiles table |
| `UserProfileSharedPrimaryKey` | Convert to shared PK pattern |
| `BackfillUserProfilesFromAspNetUsers` | Migrate existing data |
| `SlimAppUserDropProfileColumns` | Remove duplicate columns |

---

## 12. Current State & Roadmap

### ? Complete

- [x] ASP.NET Core Identity with GUID keys
- [x] JWT Bearer authentication
- [x] Role-based authorization (User, Admin, SuperAdmin)
- [x] User registration with profile creation
- [x] User login with lockout support
- [x] Profile management (get, update)
- [x] Public/private profile visibility
- [x] Global exception handling
- [x] FluentValidation pipeline
- [x] Serilog structured logging
- [x] Swagger/OpenAPI documentation
- [x] Clean Architecture structure
- [x] CQRS pattern with MediatR
- [x] Entity-specific repositories

### ?? Ready for Extension

The architecture is designed to easily support:

| Feature | Required Components |
|---------|---------------------|
| **Posts** | `Post` entity, `IPostRepository`, CQRS handlers |
| **Connections** | `Connection` entity, `IConnectionRepository`, request/accept flow |
| **Feed** | Query handlers aggregating posts from connections |
| **Messaging** | `Message` entity, real-time with SignalR |
| **Notifications** | Event-driven with MediatR notifications |

### Example: Adding Posts Feature

```
Domain/
??? Entities/
    ??? Post.cs

Application/
??? Posts/
    ??? Commands/
    ?   ??? CreatePostCommand.cs
    ?   ??? UpdatePostCommand.cs
    ?   ??? DeletePostCommand.cs
    ??? Queries/
    ?   ??? GetPostQuery.cs
    ?   ??? GetFeedQuery.cs
    ??? Handlers/
    ??? Dtos/
    ??? Validators/

Application/Common/Interfaces/
??? IPostRepository.cs

Infrastructure/Persistence/Repositories/
??? PostRepository.cs
```

---

## 13. Architectural Rules

### Must Follow

1. **Controllers are thin** — Only MediatR calls, no business logic
2. **Application layer owns interfaces** — Repositories defined in Application, implemented in Infrastructure
3. **No circular dependencies** — Domain has zero dependencies
4. **CQRS for all operations** — Commands for writes, Queries for reads
5. **FluentValidation for all input** — Validators for every request DTO
6. **Entity-specific repositories** — One interface + implementation per aggregate
7. **Global exception handling** — No try/catch in controllers
8. **Shared PK pattern** — `UserProfile.AppUserId` is both PK and FK

### Must NOT Do

1. ? Business logic in controllers
2. ? Direct DbContext usage in handlers (use repositories)
3. ? Application referencing Infrastructure
4. ? Bypassing Identity for AppUser operations
5. ? Manual validation in handlers (use pipeline)
6. ? Catching exceptions in controllers (use middleware)
7. ? Using `IBaseRepository<T>` directly (use entity-specific interface)

---

## Quick Reference

### API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | None | Register new user |
| POST | `/api/auth/login` | None | Login, get JWT |
| GET | `/api/profile/me` | Required | Get own profile |
| PUT | `/api/profile/me` | Required | Update own profile |
| GET | `/api/profile/{userId}` | None | Get public profile |
| GET | `/api/me` | Required | Get current user claims |
| POST | `/api/admin/set-role` | SuperAdmin | Assign role to user |

### Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=LinkedInCloneDb;..."
  },
  "Jwt": {
    "Key": "your-32+-character-secret-key",
    "Issuer": "LinkedInClone.Api",
    "Audience": "LinkedInClone.Client",
    "ExpiresMinutes": 60
  },
  "SeedAdmin": {
    "Email": "admin@example.com",
    "Password": "AdminPassword123!",
    "FirstName": "Super",
    "LastName": "Admin"
  }
}
```

### Running the Project

```bash
# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update --project Infrastructure --startup-project Api

# Run the API
dotnet run --project Api

# Access Swagger
# https://localhost:7xxx/swagger
```

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Authors**: LinkedInClone Development Team
