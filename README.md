# XProducts Web API

A production-ready C# Web API for managing product catalog and processing orders, built with clean architecture principles and Entity Framework Core.

---

## 🚀 Setup Instructions

1. **Clone the repository**


   XProducts Web API

A production-ready C# Web API for managing product catalog and processing orders, designed using clean architecture principles and Entity Framework Core.

🚀 Setup Instructions

Clone the repository

git clone https://github.com/<your-username>/XProducts.git
cd XProducts


# Configure Environment Variables

Create an appsettings.json (or appsettings.Development.json) with the following keys:

{
  "ConnectionStrings": {
     "Sqlite": "Data Source=app.db;",
     "DefaultConnection": "Host=localhost;Port=5432;Database=xproducts_db;Username=postgres;Password=123;"
   }
}


Install Dependencies

## dotnet restore


## Run Migrations

dotnet ef database update --project XProducts.Infrastructure --startup-project XProducts.API



## Run the API

dotnet run --project XProducts.API


## Test Endpoints

Use Postman or any API client to test endpoints:

GET /api/products – list products

GET /api/products/{id} – get a single product

POST /api/products – create a product

PUT /api/products/{id} – update a product

POST /api/orders – place an order

## 📝 Assumptions

Orders are only valid if all items are in stock; partial orders are rejected.

Product stock is decreased atomically to prevent overselling.

No authentication or authorization is implemented (assume internal API).

The API supports PostgreSQL 
## Testing 
I used  SQLite for the orderservices but  PostgreSQL  for productRepository mostly for testing the concurrency

## 🛠 Tech Stack Choices

Language: C#

Framework: .NET 9 / ASP.NET Core

Architecture: Clean Architecture (Layered + Separation of Concerns)

ORM: Entity Framework Core 9

Database: SQLite (development), PostgreSQL (production)

Testing: xUnit, Moq, EF Core PosgreSQL

Mapping: AutoMapper for DTO-to-Entity transformations

## ⚡ Important Notes

OrderService uses retry logic with transactions to handle concurrent orders safely.

Product and order DTOs are implemented using C# 9 record types for immutability.

All repositories are designed to be testable with in-memory EF Core or SQLite.

Ensure PostgreSQL service is running before switching to UseSqlite: false.

```bash
git clone https://github.com/<your-username>/XProducts.git
cd XProducts

```
# Set XProducts.API as startup Project

```# Update database
dotnet ef database update --project XProducts.Infrastructure --startup-project XProducts.API
