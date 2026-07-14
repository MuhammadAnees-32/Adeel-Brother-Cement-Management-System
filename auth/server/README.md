# Auth Server (Backend)

.NET API authentication code lives inside `src/` following the layered architecture.

## File Map

```
src/
  AdeelBrotherCement.Domain/
    Entities/AppUser.cs              User entity
    Enums/UserRole.cs                Admin, Salesman
    Enums/AppScreen.cs               Screen permissions

  AdeelBrotherCement.Application/
    DTOs/AuthDtos.cs                 Login, user DTOs
    DTOs/LoginRequest.cs             Login request
    Services/AuthenticationService.cs JWT login
    Services/UserService.cs          User CRUD + permissions
    ScreenPermissions.cs             Role/screen rules
    PasswordHasher.cs                Password hashing
    Interfaces/IRepositories.cs    IUserRepository

  AdeelBrotherCement.Infrastructure.Excel/
    ExcelUserRepository.cs           Users sheet in Excel
    ExcelWorkbookManager.cs          Users sheet seeding

  AdeelBrotherCement.Api/
    Controllers/AuthControllers.cs   /api/auth/login, /api/users
    Authorization/RequireScreenAttribute.cs
    Program.cs                       JWT + static files
    appsettings.json                 JWT config
```

## API Endpoints

| Endpoint | Access |
|----------|--------|
| POST `/api/auth/login` | Public |
| GET/POST/PUT/DELETE `/api/users` | Admin only |
| All other `/api/*` routes | JWT + screen permission |

## Data Storage

Users are stored in the `Users` sheet of `data/BusinessData.xlsx`.
