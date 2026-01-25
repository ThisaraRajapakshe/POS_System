[![Backend CI/CD](https://github.com/ThisaraRajapakshe/POS_System/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/ThisaraRajapakshe/POS_System/actions/workflows/dotnet-ci.yml)
---

# 💻 POS System Backend API

This repository contains the backend RESTful API for the Point-of-Sale (POS) System. It is built using **.NET 8**, **C#**, and **Docker**, serving as the central data and business logic hub for the entire application.

The API is responsible for:

* 🔐 **Security:** Secure user authentication and role-based authorization using **JWT**.
* 🗄️ **Data:** Managing the database schema via **Entity Framework Core**.
* 💰 **Sales:** Processing sales transactions, calculating totals, and managing order history.
* 📦 **Inventory:** Managing products, categories, and stock levels.

#### 🌐 Live Demo

The API is currently hosted live on **Microsoft Azure**.

* **Swagger UI:** **[Click here to test the Live API](https://thisara-pos-api-dddch9fhgjgka3ag.southeastasia-01.azurewebsites.net/swagger/index.html)**

#### Related Project

* **Frontend Repository:** [POS-Frontend UI (Angular)](https://pos-frontend-murex.vercel.app/login)

---

## 🛠️ Technology Stack

| Component | Technology | Description |
| --- | --- | --- |
| **Cloud Hosting** | **Microsoft Azure** | Application hosted on Azure App Service. |
| **CI/CD** | **GitHub Actions** | Automated Testing and Continuous Deployment pipeline. |
| **Framework** | .NET 8 | The core runtime and framework for building the API. |
| **Containerization** | Docker | Used for containerizing the API and Database. |
| **Database** | SQL Server | Azure SQL Database (Prod) / SQL Server Docker (Dev). |
| **API** | ASP.NET Core 8 | Used for building the RESTful API endpoints. |
| **ORM** | Entity Framework Core 8 | Manages database models, migrations, and queries. |
| **Auth** | ASP.NET Identity + JWT | Handles user management and API security. |
| **Testing** | xUnit + Moq | Comprehensive Unit Testing suite. |
| **Docs** | Swagger (OpenAPI) | Interactive API documentation. |

---

## ⚙️ CI/CD Pipeline

This project uses **GitHub Actions** for fully automated Continuous Integration and Deployment.

* **Trigger:** Pushes to the `master` branch.
* **Process:**
1. **Build:** Compiles the .NET code to ensure no syntax errors.
2. **Test:** Runs the full **xUnit** test suite.
3. **Deploy:** If (and only if) tests pass, the code is automatically deployed to **Azure App Service**.



---

## 🚀 Getting Started

You can run this project either **Locally** (using the .NET CLI) or inside a **Docker Container**.

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

### Option 1: Running Locally (Development Profile)

#### 1. Configure the Database

Ensure your SQL Server (Docker container or Local instance) is running. Update `appsettings.Development.json` if necessary.

#### 2. Apply Migrations

Create the database schema:

```bash
dotnet ef database update

```

#### 3. Run the API

```bash
dotnet run --launch-profile "LocalDev"

```

The API will start at: **`http://localhost:5050`**

---

### Option 2: Running with Docker 🐳

This project includes a `Dockerfile` for containerized deployment.

#### 1. Build the Image

Open a terminal in the root directory and run:

```bash
docker build -t pos-backend .

```

#### 2. Run the Container

```bash
docker run -d -p 8080:8080 --name pos-api pos-backend

```

* The API will be accessible at: **`http://localhost:8080`**
* *Note: Ensure your API container can talk to your SQL Server container (usually via a Docker Network).*

---

## 🧪 Running Tests

This project enforces code quality with a full Unit Test suite covering Services, Controllers, and Auth logic.

To execute the tests:

1. Navigate to the Test project:
```bash
cd POS.Tests

```


2. Run the tests:
```bash
dotnet test

```



---

## 🔑 API Documentation (Swagger)

The Swagger UI provides interactive documentation to test endpoints.

* **Live (Azure):** `https://thisara-pos-api-dddch9fhgjgka3ag.southeastasia-01.azurewebsites.net/swagger/index.html`
* **Local:** `http://localhost:5050/swagger/index.html`
* **Docker:** `http://localhost:8080/swagger/index.html`

### Authentication Flow

Most endpoints are protected (🔒). To test them:

1. Use `/api/Auth/login` to get an **AccessToken**.
2. Click **Authorize** at the top right of Swagger.
3. Enter `Bearer <your-token>`.

#### 🔐 Demo Credentials

The application database is seeded with the following default accounts for testing purposes:

| Role | Username | Password | Access Level |
| --- | --- | --- | --- |
| **Admin** | `Admin` | `Password1234!` | Full Access (Users, Inventory, Sales) |
| **Manager** | `Manager` | `Password1234!` | Inventory Management & Reports |
| **Cashier** | `Cashier` | `Password1234!` | Sales & Order Processing |

### Roles

* **Admin:** Full access to User Management, Inventory, and Sales.
* **Manager:** Manage Inventory and view Reports.
* **Cashier/StockClerk:** Create Orders and update specific items.

---

## 👤 Developer

* **Author:** RKD Thisara Sandeep
* **Version Control:** Managed with Git and hosted on GitHub.
