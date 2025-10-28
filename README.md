
# 💻 POS System Backend API

This repository contains the backend RESTful API for the Point-of-Sale (POS) System. It is built using **.NET 8** and **C\#**, and it serves as the central data and business logic hub for the entire application.

The API is responsible for:

  * Secure user authentication and role-based authorization using **JWT**.
  * Managing the database schema via **Entity Framework Core**.
  * Processing all business logic for sales, inventory, and catalog management.

### Related Project

  * **Frontend Repository:** [POS-Frontend UI (Angular)](https://github.com/ThisaraRajapakshe/POS-Frontend)
      * **Note:** This API must be running for the frontend application to function correctly.

-----

## 🛠️ Technology Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | **.NET 8** | The core runtime and framework for building the API. |
| **Language** | **C\# 12** | The modern, object-oriented language used for all logic. |
| **API** | **ASP.NET Core 8** | Used for building the RESTful API endpoints. |
| **Database ORM** | **Entity Framework Core 8** | Manages the database (models, migrations, queries). |
| **Database** | **SQL Server** | The relational database storing all application data. |
| **Authentication** | **ASP.NET Identity** | Manages user accounts, passwords, and roles. |
| **Authorization** | **JWT Bearer Tokens** | Secures API endpoints using industry-standard JSON Web Tokens. |
| **API Docs** | **Swagger (OpenAPI)** | Provides interactive API documentation and testing. |

-----

## 🚀 Getting Started

Follow these instructions to get the backend API running on your local machine for development and testing.

### Prerequisites

You must have the following tools installed on your local machine:

  * [.NET 8 SDK](https://dotnet.microsoft.com/download)
  * **SQL Server** (e.g., [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) or [Developer Edition](https://www.microsoft.com/sql-server/sql-server-downloads))
  * An API testing tool like [Postman](https://www.postman.com/) (optional, as Swagger is built-in)

### 1\. Configure the Database Connection

The application needs to know where to find your SQL Server instance.

1.  Open the `appsettings.Development.json` file.
2.  Find the `ConnectionStrings` section.
3.  Update the `DefaultConnection` value to point to your local SQL Server instance. It should look something like this:

<!-- end list -->

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PosDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
}
```

### 2\. Set Up the Database (EF Core Migrations)

This command will read all the C\# model configurations from Entity Framework Core and automatically create the database and all its tables for you.

1.  Open a terminal in the root of the project directory.
2.  Run the following command:

<!-- end list -->

```bash
dotnet ef database update
```

*(If you get an error, you may need to install the EF Core tool first by running: `dotnet tool install --global dotnet-ef`)*

### 3\. Run the Application

Execute the application from the root directory:

```bash
dotnet run
```

The API will start and listen on the configured ports (e.g., `https://localhost:7001` and `http://localhost:5001`).

-----

## 🔑 API & Security

### API Endpoint Testing (Swagger)

Once the application is running, the **Swagger UI** is the easiest way to explore and test all available endpoints.

Navigate to: **`https://localhost:7001/swagger/index.html`**

You will see a complete, interactive list of all API endpoints, their required parameters, and example responses.

### Authentication (JWT)

This API is secured using JSON Web Tokens (JWT). All requests to protected endpoints must include a valid Bearer Token in the `Authorization` header.

**To get a token for testing in Swagger:**

1.  Expand the `/api/Auth/login` endpoint.
2.  Click "Try it out" and provide the credentials for a user in your database (you may need to register one first or seed the database).
3.  Execute the request. The response body will contain an `accessToken`.
4.  Copy this token.
5.  At the top right of the Swagger page, click the **"Authorize"** button.
6.  In the popup, paste the token into the `Value` box (prefixed with ` Bearer  `) and click "Authorize".
7.  You can now test all the locked (🔒) endpoints.

### Role-Based Authorization (RBAC)

This API uses role-based authorization to restrict access to sensitive operations.

  * Endpoints are decorated with attributes like `[Authorize(Roles = "Admin")]` or `[Authorize(Roles = "Cashier")]`.
  * This ensures that only users with the correct role (as defined in their JWT) can access specific resources. For example, a "Cashier" cannot access admin-only endpoints like `/api/Users`.

**To check if RBAC is working:**

1.  Log in as a user with the "Cashier" role and get their token.
2.  Authorize using that token in Swagger.
3.  Attempt to call an Admin-only endpoint (e.g., `GET /api/Users`).
4.  You should receive a **`403 Forbidden`** response, which confirms your security is working correctly.

-----

## 👤 Developer

  * **Author:** RKD Thisara Sandeep
  * **Version Control:** Managed with Git and hosted on GitHub.
