# Webshop.API

A RESTful webshop API built with **ASP.NET Core 10** and the **Regira Entities** framework. Customers can browse categorised products, filter by category, price, and stock, and submit orders.

---

## Table of Contents

- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
  - [Categories](#categories)
  - [Products](#products)
  - [Customers](#customers)
  - [Orders](#orders)
- [Filtering & Searching](#filtering--searching)
- [Common Patterns for SPA Consumers](#common-patterns-for-spa-consumers)
- [Sample Data](#sample-data)
- [Project Structure](#project-structure)
- [Credits](#credits)

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run

```bash
cd Webshop.API
dotnet run
```

The API starts on `https://localhost:5001` (or `http://localhost:5000`).  
Interactive API documentation (Scalar UI) is available at:

```
https://localhost:5001/scalar/v1
```

The SQLite database (`webshop.db`) is created automatically on first run and seeded with:
- 8 categories
- 41 products
- 20 customers
- ~102 orders

---

## API Endpoints

All endpoints follow the Regira Entities convention. Each resource exposes:

| Method   | Route              | Description                      |
|----------|--------------------|----------------------------------|
| `GET`    | `/{id}`            | Get single item by ID            |
| `GET`    | `/`                | List all items (no filter)       |
| `POST`   | `/list`            | List with filter in request body |
| `GET`    | `/search`          | Search with count (query string) |
| `POST`   | `/search`          | Search with count (body)         |
| `POST`   | `/`                | Create new item                  |
| `POST`   | `/save`            | Upsert (create or update)        |
| `PUT`    | `/{id}`            | Full update                      |
| `PATCH`  | `/{id}`            | Partial update (JSON Merge Patch)|
| `DELETE` | `/{id}`            | Delete item                      |

---

### Categories

**Base route:** `/categories`

#### SearchObject fields

| Field       | Type       | Description                                 |
|-------------|------------|---------------------------------------------|
| `q`         | `string`   | Full-text search (title + description)      |
| `ids`       | `int[]`    | Filter by specific IDs                      |
| `parentId`  | `int[]`    | Categories that have these parent IDs       |
| `childId`   | `int[]`    | Categories that have these child IDs        |
| `isRoot`    | `bool`     | `true` = top-level only, `false` = sub only |

#### Includes (query string `?includes=`)

| Value      | Description                                   |
|------------|-----------------------------------------------|
| `Parents`  | Include parent category relationships         |
| `Children` | Include child category relationships          |
| `All`      | Include both parents and children             |

**Each category DTO includes a `productCount` field** filled automatically.

---

### Products

**Base route:** `/products`

#### SearchObject fields

| Field        | Type       | Description                                   |
|--------------|------------|-----------------------------------------------|
| `q`          | `string`   | Full-text search (title + description)        |
| `ids`        | `int[]`    | Filter by specific IDs                        |
| `categoryId` | `int[]`    | Filter products belonging to these categories |
| `minPrice`   | `decimal`  | Minimum price                                 |
| `maxPrice`   | `decimal`  | Maximum price                                 |
| `inStock`    | `bool`     | `true` = only products with stock > 0         |

#### SortBy values

| Value       | Description           |
|-------------|-----------------------|
| `Default`   | Title A–Z             |
| `Title`     | Title A–Z             |
| `TitleDesc` | Title Z–A             |
| `Price`     | Price low–high        |
| `PriceDesc` | Price high–low        |
| `Newest`    | Newest first          |
| `Oldest`    | Oldest first          |

#### Includes

| Value | Description                               |
|-------|-------------------------------------------|
| `All` | Include product categories with category details |

---

### Customers

**Base route:** `/customers`

> Customer IDs are **GUIDs** (not integers).

#### SearchObject fields

| Field | Type     | Description            |
|-------|----------|------------------------|
| `q`   | `string` | Full-text search (name + email) |
| `ids` | `guid[]` | Filter by specific IDs |

#### Input model

```json
{
  "name": "Alice Johnson",
  "email": "alice@example.com",
  "phone": "+32 470 000 001"
}
```

---

### Orders

**Base route:** `/orders`

#### SearchObject fields

| Field        | Type            | Description                                |
|--------------|-----------------|--------------------------------------------|
| `q`          | `string`        | Full-text search                           |
| `ids`        | `int[]`         | Filter by specific order IDs               |
| `code`       | `string`        | Exact order code match                     |
| `customerId` | `guid[]`        | Filter by customer GUIDs                   |
| `productId`  | `int[]`         | Orders containing these products           |
| `categoryId` | `int[]`         | Orders containing products from categories |
| `status`     | `OrderStatus[]` | Filter by order status                     |

#### OrderStatus values

| Value        | Meaning                   |
|--------------|---------------------------|
| `Pending`    | Awaiting processing       |
| `Processing` | Being prepared/packed     |
| `Shipped`    | Dispatched to carrier     |
| `Delivered`  | Received by customer      |
| `Cancelled`  | Order was cancelled       |

#### Includes

| Value        | Description                              |
|--------------|------------------------------------------|
| `Customer`   | Embed full customer object               |
| `OrderLines` | Embed order lines with product details   |
| `All`        | Include both customer and order lines    |

#### Submit an order

```http
POST /orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "shippingAddress": "42 Main Street, Brussels",
  "orderLines": [
    { "productId": 1, "quantity": 2, "unitPrice": 249.99 },
    { "productId": 4, "quantity": 1, "unitPrice": 49.99 }
  ]
}
```

The order `code`, `total`, and line `subTotal` values are computed automatically.

---

## Filtering & Searching

### List vs Search

- **`/list`** — returns an array of items.
- **`/search`** — returns `{ items: [...], count: N }` — use this when you need pagination counts.

### Paging

Pass `page` and `pageSize` in the search object body or query string:

```json
{
  "q": "headphones",
  "categoryId": [1],
  "page": 1,
  "pageSize": 12
}
```

### Full-text search

All entities support the `q` field for full-text search over normalised content. Example:

```http
POST /products/search
Content-Type: application/json

{
  "q": "wireless bluetooth",
  "minPrice": 50,
  "maxPrice": 200,
  "sortBy": "Price"
}
```

---

## Common Patterns for SPA Consumers

### Load products for a category page

```js
// GET /products?categoryId=1&includes=All&sortBy=Price&pageSize=12
const res = await fetch('/products/search', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    categoryId: [1],
    inStock: true,
    sortBy: 'Price',
    pageSize: 12,
    page: 1,
  }),
});
const { items, count } = await res.json();
```

### Load root categories for navigation

```js
const res = await fetch('/categories/list', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ isRoot: true }),
});
const categories = await res.json();
```

### Submit an order

```js
const res = await fetch('/orders', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    customerId: currentUser.id,
    shippingAddress: cart.shippingAddress,
    orderLines: cart.items.map(item => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.price,
    })),
  }),
});
const order = await res.json();
```

### Get a customer's order history

```js
const res = await fetch('/orders/search', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    customerId: [currentUser.id],
    includes: 'OrderLines',
    sortBy: 'Default',
    pageSize: 20,
    page: 1,
  }),
});
const { items, count } = await res.json();
```

---

## Sample Data

On first startup the API seeds:

| Entity    | Count |
|-----------|-------|
| Categories | 8 |
| Products  | 41 |
| Customers | 20 |
| Orders    | ~102 |

Products span all 8 categories. Orders are distributed across all customers with 1–4 order lines each, and carry randomised statuses and dates spread over the last year.

---

## Project Structure

```
Webshop.API/
├── Controllers/          # EntityControllerBase-derived controllers
├── Data/
│   └── WebshopDbContext.cs
├── Entities/
│   ├── Categories/       # Category, RelatedCategory, DTOs, SearchObject, Processor, ServiceConfig
│   ├── Customers/        # Customer, DTOs, ServiceConfig
│   ├── Orders/           # Order, OrderLine, DTOs, SearchObject, Includes, QueryBuilder, ServiceConfig
│   └── Products/         # Product, ProductCategory, DTOs, SearchObject, SortBy, QueryBuilder, ServiceConfig
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Seeding/
│   └── DataSeeder.cs
├── Program.cs
├── appsettings.json
├── NuGet.Config
└── Webshop.API.csproj
```

---

## Credits

Built with:

- [ASP.NET Core 10](https://learn.microsoft.com/aspnet/core)
- [Regira Entities](https://regira.com) — entity framework for rapid CRUD API development
- [Entity Framework Core](https://learn.microsoft.com/ef/core) with SQLite
- [Scalar](https://scalar.com) — OpenAPI UI
- [Mapster](https://github.com/MapsterMapper/Mapster) — object mapping

> **AI Agent:** This project was scaffolded and implemented by **Claude Sonnet 4.6** (Anthropic), running as an autonomous agent via the **Claude Agent SDK** inside the Claude Code CLI.
