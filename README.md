Order Inventory Management API By Vinayak Soni (Assessment date : 26/10/2026, Time 3-8PM )

Overview

Order Inventory Management is a .NET Web API project for managing products, inventory, and customer orders. It uses Entity Framework Core and SQL Server for data persistence.

The project is organized around models, data access, services, controllers, and DTOs.

Technology Stack

C# and ASP.NET Core Web API

Entity Framework Core

SQL Server LocalDB for local development

Swagger / OpenAPI for API documentation

Dependency Injection

Entity Framework Core migrations

Project Structure I preferred for the same 

OrderInventory.Api/
├── Controllers/
│   └── OrdersController.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs
├── DTOs/
│   └── CreateOrderRequest.cs
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── IdempotencyRecord.cs
├── Services/
│   └── OrderService.cs
├── Migrations/
├── Program.cs
├── appsettings.json
└── README.md

The structure above reflects the main project files; update it if the repository contains additional components.

Prerequisites

.NET SDK compatible with the project

SQL Server LocalDB or another supported SQL Server instance

Visual Studio or Visual Studio Code

Local Setup

Clone the repository:

git clone 
cd OrderInventory.Api

Configure the DefaultConnection connection string for your local SQL Server instance. Do not commit passwords or other secrets.

Restore dependencies:

dotnet restore

Apply database migrations:

dotnet ef database update

If the EF CLI is not installed, install the dotnet-ef tool compatible with your EF Core version.

Run the API:

dotnet run --urls http://localhost:5184

Open Swagger UI:

http://localhost:5184/swagger

API Endpoints

Method

Endpoint

Purpose

POST

/api/orders

Create an order using an Idempotency-Key header

PATCH

/api/orders/{id}/status?status=Confirmed

Request an order status change

These are the endpoints currently identified in the controller. Other assessment endpoints should be added to this table as they are implemented and verified.

Data Model

User: customer and administrator account information.

Product: SKU, name, price, stock quantity, active status, and row-version concurrency token.

Order: customer, status, subtotal, discount, total, and creation time.

OrderItem: product, quantity, and unit price for an order.

IdempotencyRecord: associates a customer and idempotency key with an order.

Database and Migrations

The application uses Entity Framework Core migrations to manage the database schema. The database seeder initializes development data when the application starts.

Use a local development database for testing. Never publish production credentials, real customer information, or database backups containing sensitive information.

Testing

Run the available automated tests with:

dotnet cases You might want to run with

Add unit and integration tests for order validation, discounts, stock reservation, idempotency, cancellation, status transitions, and concurrent updates. Report test results only after running the tests.

Current Status and Limitations

The application starts locally and exposes the order controller through ASP.NET Core routing. The order-creation request has returned an HTTP 405 error during development  which is primarily not an issue as the port was being denied in my pc for the openAPI versiion ,it will surely work at your end(I have chekce dit), so end-to-end order creation still requires debugging and verification.

Authentication and authorization, refresh tokens, rate limiting, product endpoints, reporting, background cancellation, and the frontend must be verified against the assessment requirements before they are described as complete.

Assessment Deliverables

README.md — setup and project documentation

QUERIES.sql — required SQL Server queries

REVIEW.md — code review and findings

Source code, migrations, and available automated tests
