# Auth Module

Authentication and role-based access for Adeel & Brother Cement.

## Folder Structure

```
auth/
  client/     React login, permissions, protected routes
  server/     .NET API auth (lives in src/, documented here)
  README.md
```

## Roles

| Role | Access |
|------|--------|
| **Admin** | All screens + user management |
| **Salesman** | Configurable screens (default: New Sale, Inventory, Customer Balance, Expenses) |

## Default Logins

| Username | Password |
|----------|----------|
| admin | Admin@123 |
| MuhammadAnees | MAnees@2026! |

## Screens

- Dashboard
- New Sale
- Sales History
- Customer Balance
- Inventory
- Expenses
- User Management (Admin only)

Users are stored in the `Users` sheet inside `data/BusinessData.xlsx`.

## First-time setup (after git clone)

```powershell
cd client
npm install
cd ..
powershell -ExecutionPolicy Bypass -File scripts/setup-auth.ps1
```

This links `auth/client/node_modules` to `client/node_modules` so TypeScript can compile the auth module.
