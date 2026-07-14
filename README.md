
# Adeel & Brother Cement — Business Management App

A .NET + React application for managing daily sales, inventory, expenses, and profit for a Cement & Sirya agency.

## Tech Stack

- **Backend:** .NET 8 Web API
- **Frontend:** React + TypeScript (Vite)
- **Data:** Excel workbook (`data/BusinessData.xlsx`) — ready to move to Google Drive or SQL later

## Features

- **Stock Management** — Cement (Charat, Bestway, Fecto, Fugi, Askri, Kohat), Sirya (2mm–6mm, Ring), Taar, Keel
- **Daily Sales** — Customer transactions with auto slip generation
- **Inventory** — Stock levels, purchase/sale prices, low-stock alerts
- **Dashboard** — Sales by day/week/month/year, profit, inventory status
- **Expenses** — Daily expense tracking

## Getting Started

### 1. Run the API

```powershell
cd "d:\Adeel & Brother Cement\src\AdeelBrotherCement.Api"
dotnet run
```

API runs at `http://localhost:5049`. On first run, it creates `data/BusinessData.xlsx` with all products pre-loaded.

### 2. Run the React App

```powershell
cd "d:\Adeel & Brother Cement\client"
npm run dev
```

Open `http://localhost:5173` in your browser.

## Project Structure

```
auth/                                 # Authentication module
  client/                             # Login, roles, screen permissions (React)
  server/                             # Backend auth docs (code in src/)
client/                               # React frontend
data/                                 # Excel workbook (auto-created)
scripts/                              # Build & setup scripts
src/
  AdeelBrotherCement.Domain/          # Entities & enums
  AdeelBrotherCement.Application/     # Services, DTOs, repository interfaces
  AdeelBrotherCement.Infrastructure.Excel/  # Excel data layer
  AdeelBrotherCement.Api/             # REST API
deploy/                               # Packaged app (after running scripts/publish.ps1)
```

## Future: Google Drive & Database

The repository pattern (`IProductRepository`, `ITransactionRepository`, etc.) lets you swap Excel for:

1. **Google Drive** — Sync `BusinessData.xlsx` via Google Drive API
2. **SQL Database** — Add `Infrastructure.Sql` with Entity Framework Core

No business logic changes needed — only the data layer.

## Default Product Prices

Sample buy/sell prices are pre-seeded. Update them in **Inventory** page with your actual rates.
# Adeel-Brother-Cement-Management-System
A modern full-stack Cement Shop Management System built with ASP.NET Core (.NET 8) and React for inventory, sales, customer management, expenses, payments, and business analytics.

