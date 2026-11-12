# Inventory API

A RESTful web API for inventory management, built with C# and ASP.NET Core.

## Features

- **Product Management:** Create, retrieve, update, and delete products.
- **Category Management:** Create, retrieve, update, and delete product categories.
- **Inventory Entries:** Increase product stock while validatiing available quantities.
- **Inventory Exits:** Decrease product stock while validating available quantities.
- **Stock Validation:** Prevent inventory exits when the requested quantity exceeds available stock.
- **Inventory History:** Retrieve inventory movements and filter them by product.
- **Category Reassigment:** Reassign products to another category before deleting an existing category.
- **Data Validation:** Validate incoming requests using DTOs and data annotations.
- **Automated Testing:** Unit tests and integration tests covering controllers, services, and API behavior.

## Technologies

- **Language:** C#
- **Framework:** ASP.NET Core 10
- **Database:** SQLite
- **ORM:** Entity Framework Core 10
- **API Documentation:** OpenAPI
- **Testing:** xUnit
- **Integration Testing:** ASP.NET Core WebApplicationFactory

## Project Structure

The solution is organized into three projects:

- **Inventory API:** Main ASP.NET Core Web API project.
    - Controllers/:Handles HTTP requests and API endpoints.
    - Data/: Contains the Entity Framework Core database context.
    - DTOs/: Defines Data Transfer Objects for API requests and response.
    - Models/: Contains the application data models.
    - Services/: Implements the business logic.
    - Migrations/: Contains Entity Framework Core database migrations.

- **InventoryApi.Tests:** Contains unit tests for services and controlles.

- **IntentoryApi.IntegrationTests:** Contains integration test that verify API behavior using an isolated SQLite database.

## Requirements

To run this project, you need:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An IDE or code editor such as Visual Studio, Visual Studio Code, or JetBrains Rider.
- SQLite (The database is included through the Entity Framework Core SQLite provider).

## Installation and Setup

1. Navigate to the project directory:

cd "Inventory API"


2. Restore the project dependencies:

dotnet restore


3. Apply the database migrations:

dotnet ef database update


4. Run the API:

dotnet run


## API Endpoints

### Products

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/products | Retrieve all products |
| GET | /api/products/{id} | Retrieve a product by ID |
| POST | /api/products | Create a new product |
| PUT | /api/products/{id} | Update an existing product |
| DELETE | /api/products/{id} | Delete a product |

### Categories

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/categories | Retrieve all categories |
| GET | /api/categories/{id} | Retrieve a category by ID |
| POST | /api/categories | Create a new category |
| PUT | /api/categories/{id} | Update an existing category |
| DELETE | /api/categories/{id} | Delete a category and reassign its products |

### Inventory Movements

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/inventory/movements | Retrieve all inventory movements |
| GET | /api/inventory/products/{productId}/movements | Retrieve movements for a specific product |
| POST | /api/inventory/entries | Register an inventory entry |
| POST | /api/inventory/exits | Register an inventory exit |

## Running Tests


The solution includes unit tests and integration tests built with xUnit.

To run all tests, execute the following command from the solution directory:

dotnet test


The test suite includes:

- **69 Unit Tests:** Verify service logic and controller behavior.
- **35 Integration Tests:** Verify API endpoints, HTTP responses, database operations, and inventory workflows.

All integration tests use an isolated in-memory SQLite database to prevent test data from affecting other tests.


## Future Improvements



- Add authentication and authorization.
- Implement pagination and filtering for product and inventory movement queries.
- Improve API documentation with interactive Swagger UI.
- Add logging and centralized exception handling.
- Implement additional validation and error responses.
