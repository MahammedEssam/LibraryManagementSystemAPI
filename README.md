# Library Management System API

A RESTful backend API for managing a library's books, authors, members, and loan (borrowing) operations — built with ASP.NET Core Web API, a 3-Tier Architecture, and raw ADO.NET with Stored Procedures.

> Portfolio project built to practice RESTful API design, SOLID principles, layered architecture, and real-world concurrency handling (database transactions, race condition prevention).

---

## Tech Stack

- **Framework:** ASP.NET Core Web API (.NET 8.0)
- **Database:** SQL Server
- **Data Access:** ADO.NET (raw `SqlConnection` / `SqlCommand`) with Stored Procedures — no ORM
- **Architecture:** 3-Tier (Presentation / Business Logic / Data Access)
- **API Testing:** Swagger UI (OpenAPI)

---

## Architecture

The solution is split into three separate class library projects, each with a single responsibility:

```
LibraryManagementSystem (Solution)
│
├── LibraryManagement.API/      # Presentation Layer — Controllers, DTOs, Program.cs
├── LibraryManagement.BLL/      # Business Logic Layer — validation & business rules
└── LibraryManagement.DAL/      # Data Access Layer — ADO.NET + Stored Procedures
```

**Flow:** `Controller → BLL → DAL → Stored Procedure → Database`

Each layer only knows about the layer directly below it, which keeps business rules independent from how data is stored, and keeps data access independent from how it's exposed over HTTP.

---

## Entities & Relationships

- **Book** — belongs to one `Author`, tracks `TotalCopies` / `AvailableCopies`
- **Author** — can have many `Books` (One-to-Many)
- **Member** — a registered library user (unique email)
- **Loan** — links a `Book` and a `Member`; tracks `LoanDate`, `DueDate`, `ReturnDate`, and `Status` (Active / Returned)

---

## API Endpoints

### Books
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/book` | Get all books |
| GET | `/api/book/{id}` | Get a book by id |
| POST | `/api/book` | Add a new book |
| PUT | `/api/book/{id}` | Update a book |
| DELETE | `/api/book/{id}` | Delete a book *(blocked if it has any loan history)* |

### Authors
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/authors` | Get all authors |
| GET | `/api/authors/{id}` | Get an author by id |
| POST | `/api/authors` | Add a new author |
| PUT | `/api/authors/{id}` | Update an author |
| DELETE | `/api/authors/{id}` | Delete an author *(blocked if they have linked books)* |

### Members
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/members` | Get all members |
| GET | `/api/members/{id}` | Get a member by id |
| POST | `/api/members` | Register a new member *(email must be unique)* |
| PUT | `/api/members/{id}` | Update a member |
| DELETE | `/api/members/{id}` | Delete a member *(blocked if they have any loan history)* |

### Loans
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/loans/borrow` | Borrow a book |
| PUT | `/api/loans/{id}/return` | Return a borrowed book |

---

## Business Rules

- A book can only be borrowed if `AvailableCopies > 0`.
- A member cannot have more than **3 active loans** at the same time.
- A member cannot borrow the **same book twice** while an existing loan on it is still active (not yet returned).
- A loan's due date defaults to **14 days** from the borrow date (configurable per request).
- A book already returned cannot be returned again.
- **Deletion is protected at two levels:**
  1. The Business Logic Layer checks for *active* loans first and returns a clear `400` message.
  2. As a safety net, the Data Access Layer also catches SQL Server's Foreign Key violation (error `547`) and converts it into a clear `400` response — this covers *historical* (already-returned) loan records too, since the database won't allow deleting a book/member that still has any loan record pointing to it.
- Borrow/Return operations run inside **database transactions** with `UPDLOCK` / `HOLDLOCK` row locking to prevent race conditions under concurrent requests (e.g., two members trying to borrow the last available copy at the same moment).
- Business-rule violations return `400 Bad Request` with a descriptive message (via a custom `BusinessException`); unexpected technical failures return `500 Internal Server Error` without leaking internal details.

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full instance)
- SQL Server Management Studio (SSMS) — optional, for running the DB scripts

### 1. Clone the repository
```bash
git clone <your-repo-url>
cd LibraryManagementSystem
```

### 2. Create the database
Run the SQL scripts (found in `/Database` or as provided) in SSMS to:
- Create the `LibraryDB` database
- Create all tables (`Books`, `Authors`, `Members`, `Loans`)
- Create all Stored Procedures

### 3. Configure the connection string securely
Do **not** commit real connection strings to source control. Recommended approach:

- Keep a placeholder in `appsettings.json`:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```
- Put your real local value in `appsettings.Development.json` instead, and add this line to `.gitignore` so it never gets pushed:
  ```
  appsettings.Development.json
  ```
- Alternatively, use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development.

### 4. Run the project
```bash
cd LibraryManagement.API
dotnet run
```

### 5. Open Swagger
Navigate to:
```
https://localhost:<port>/swagger
```
to explore and test all endpoints interactively.

---

## Project Status

Core functionality (Books, Authors, Members, Loans with full business rules and concurrency safety) is complete and tested via Swagger. Authentication/Authorization is a planned future addition.

---

## Author

**Mohammed Essam** — Junior Backend Developer
