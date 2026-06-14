# Webshop.API

A RESTful API for a webshop built with **ASP.NET Core (.NET 10)** and the **Regira Entities** framework.  
Customers can browse categorised products and submit orders. The API is fully self-contained: it uses an SQLite database (created automatically on first run) and seeds ~110 sample orders on startup.

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- The Regira private NuGet feed is already wired in `NuGet.Config` at the solution root.

### Run

```bash
cd Webshop.API
dotnet run
```

The Scalar API explorer opens automatically at `https://localhost:7200/scalar`.  
The SQLite database (`webshop.db`) and sample data are created on first startup.

---

## Domain model

| Entity | Key | Description |
|--------|-----|-------------|
| `Category` | `int` | Product category (Electronics, Clothing, …) |
| `Product` | `int` | A sellable item; belongs to one or more categories via `ProductCategory` |
| `Customer` | `Guid` | Registered shopper |
| `Order` | `int` | A customer's purchase; holds one or more `OrderLine` items |
| `OrderLine` | `int` | A single product + quantity + unit price inside an order |

### Category → Product (many-to-many)

A product can appear in multiple categories. The `ProductCategory` join table links them.  
Filter products by one or more `CategoryId` values via the product search endpoint.

### Customer → Order → OrderLine → Product

Each order belongs to exactly one customer and contains one or more order lines referencing products.  
The `Total` on `Order` is calculated automatically from the lines on every save.

---

## API reference

The full, interactive API reference is available at `/scalar` when the app is running.  
All endpoints return JSON with null fields omitted and enum values serialised as strings.

### Categories  `/categories`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/categories` | List all categories (supports `?q=` text search) |
| `GET` | `/categories/{id}` | Category details |
| `POST` | `/categories` | Create a new category |
| `PUT` | `/categories/{id}` | Full update |
| `PATCH` | `/categories/{id}` | Partial update (JSON Merge Patch) |
| `DELETE` | `/categories/{id}` | Delete |
| `POST` | `/categories/save` | Upsert (create or update) |

**Filter parameters (query string or POST body)**

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | `string` | Text search on title and description |
| `id` | `int` | Filter by exact id |
| `ids` | `int[]` | Filter by id list |
| `isArchived` | `bool` | Show only archived / non-archived |

---

### Products  `/products`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/products` | List products |
| `POST` | `/products/list` | List products (filter in body) |
| `GET` | `/products/search` | Search with total count |
| `POST` | `/products/search` | Search (filter in body) |
| `GET` | `/products/{id}` | Product details |
| `POST` | `/products` | Create |
| `PUT` | `/products/{id}` | Full update |
| `PATCH` | `/products/{id}` | Partial update |
| `DELETE` | `/products/{id}` | Delete |
| `POST` | `/products/save` | Upsert |

**Filter / sort parameters**

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | `string` | Full-text search on title and description |
| `categoryId` | `int[]` | Filter by one or more category ids |
| `minPrice` | `decimal` | Minimum price |
| `maxPrice` | `decimal` | Maximum price |
| `inStock` | `bool` | Only show products with `Stock > 0` |
| `sortBy` | `ProductSortBy` | `Default`, `Title`, `TitleDesc`, `Price`, `PriceDesc`, `Newest` |
| `includes` | `EntityIncludes` | `All` to include category details in the response |
| `page` | `int` | Page number (default 1) |
| `pageSize` | `int` | Page size (default 50, max 200) |

**Create / update body** (`ProductInputDto`)

```json
{
  "title": "Wireless Keyboard",
  "description": "Compact Bluetooth keyboard for desk and travel.",
  "price": 44.99,
  "stock": 75,
  "categories": [
    { "categoryId": 1 }
  ]
}
```

---

### Customers  `/customers`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/customers` | List customers |
| `GET` | `/customers/{id}` | Customer details (id is a GUID) |
| `POST` | `/customers` | Create |
| `PUT` | `/customers/{id}` | Full update |
| `PATCH` | `/customers/{id}` | Partial update |
| `DELETE` | `/customers/{id}` | Delete |
| `POST` | `/customers/save` | Upsert |

**Create / update body** (`CustomerInputDto`)

```json
{
  "name": "Jane Doe",
  "email": "jane.doe@example.com",
  "phone": "+1-800-555-0100",
  "address": "42 Acacia Avenue, Springfield"
}
```

---

### Orders  `/orders`

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/orders` | List orders |
| `POST` | `/orders/list` | List orders (filter in body) |
| `GET` | `/orders/search` | Search with total count |
| `POST` | `/orders/search` | Search (filter in body) |
| `GET` | `/orders/{id}` | Order details (always includes customer + lines + products) |
| `POST` | `/orders` | Create a new order |
| `PUT` | `/orders/{id}` | Full update |
| `PATCH` | `/orders/{id}` | Partial update |
| `DELETE` | `/orders/{id}` | Delete |
| `POST` | `/orders/save` | Upsert |

**Filter parameters**

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | `string` | Text search |
| `code` | `string` | Exact order code, e.g. `ORD-20240615120000123` |
| `customerId` | `Guid[]` | Filter by customer |
| `productId` | `int[]` | Orders containing a specific product |
| `categoryId` | `int[]` | Orders containing a product in a specific category |
| `status` | `OrderStatus[]` | `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled` |
| `minCreatedDate` | `DateTime` | Earliest order date |
| `maxCreatedDate` | `DateTime` | Latest order date |

**Create body** (`OrderInputDto`)

```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "notes": "Please leave at the door.",
  "orderLines": [
    { "productId": 1, "quantity": 2, "unitPrice": 89.99 },
    { "productId": 4, "quantity": 1, "unitPrice": 34.99 }
  ]
}
```

> The `code` and `total` fields are computed automatically on the server.

---

## Paging

All list and search endpoints support paging via query-string parameters:

| Parameter | Default | Description |
|-----------|---------|-------------|
| `page` | `1` | 1-based page number |
| `pageSize` | `50` | Items per page (max 200) |

---

## Sample data

On first startup the API seeds:

- **8 categories** (Electronics, Clothing, Books, Home & Garden, Sports & Outdoors, Toys & Games, Health & Beauty, Food & Drink)
- **25 products** spread across categories, with realistic prices and stock levels
- **10 customers** with names, emails, phone numbers and addresses
- **~110 orders** distributed across all customers, each with 1–3 order lines, assigned random statuses

---

## Architecture notes

The project follows the **Regira Entities** framework conventions:

- **`Regira.Entities.Web`** — `EntityControllerBase<>` provides all CRUD + search endpoints out of the box.
- **`Regira.Entities.Mapping.Mapster`** — DTO mapping handled automatically by Mapster convention; only non-trivial nested types require `AddMapping<>()`.
- **SQLite + EF Core** — The database is created with `EnsureCreated()` on startup; no migrations required for development. Switch to PostgreSQL or SQL Server by changing the connection string and EF Core provider.
- **Serilog** — Structured logging to console and a rolling file under `logs/`.
- **Scalar** — Interactive API explorer at `/scalar` (replaces Swagger UI).

### Tier budget (Regira free tier: 5 simple + 2 complex entity registrations)

| Entity | Registration | Tier slot |
|--------|-------------|-----------|
| `Category` | `For<Category, int, CategorySearchObject>()` | Simple #1 |
| `Customer` | `For<Customer, Guid>()` | Simple #2 |
| `Product` | `For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>()` | Complex #1 |
| `Order` | `For<Order, OrderSearchObject, EntitySortBy, OrderIncludes>()` | Complex #2 |

---

## Credits

Generated with assistance from **Claude Sonnet 4.6** (claude-sonnet-4-6) using the **Claude Code** agent (VS Code extension, autonomous agent mode) guided by the Regira MCP server for package discovery and API reference.
