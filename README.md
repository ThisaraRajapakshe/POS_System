
---

```markdown
[![Backend CI/CD](https://github.com/ThisaraRajapakshe/POS_System/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/ThisaraRajapakshe/POS_System/actions/workflows/dotnet-ci.yml)

---

# 💻 POS System Backend API

This repository contains the backend RESTful API for the Point-of-Sale (POS) System. It is built using **.NET 8**, **C#**, and **Docker**, serving as the central data and business logic hub for the entire application.

The API is responsible for:

- 🔐 **Security:** Secure user authentication and role-based authorization using **JWT**.
- 🗄️ **Data:** Managing the database schema via **Entity Framework Core**.
- 💰 **Sales:** Processing sales transactions, calculating totals, and managing order history.
- 📦 **Inventory:** Managing products, categories, and stock levels.

#### 🌐 Live Demo

The API is currently hosted live on **AWS Lightsail**.

- **Swagger UI:** [Click here to test the Live API](https://www.thisara.dev/api/swagger/index.html)
- **Frontend App:** [POS Frontend (Angular)](https://www.thisara.dev)

---

## 🛠️ Technology Stack

| Component | Technology | Description |
| --- | --- | --- |
| **Cloud Hosting** | **AWS Lightsail** | Application hosted on AWS Lightsail (EC2-based VPS). |
| **CI/CD** | **GitHub Actions** | Automated Testing and Continuous Deployment pipeline. |
| **Framework** | .NET 8 | The core runtime and framework for building the API. |
| **Containerization** | Docker | Used for containerizing the API and Database. |
| **Database** | SQL Server | SQL Server Express running in Docker container. |
| **API** | ASP.NET Core 8 | Used for building the RESTful API endpoints. |
| **ORM** | Entity Framework Core 8 | Manages database models, migrations, and queries. |
| **Auth** | ASP.NET Identity + JWT | Handles user management and API security. |
| **Testing** | xUnit + Moq | Comprehensive Unit Testing suite. |
| **Docs** | Swagger (OpenAPI) | Interactive API documentation. |

---

## ⚙️ CI/CD Pipeline

This project uses **GitHub Actions** for fully automated Continuous Integration and Deployment.

- **Trigger:** Pushes to the `master` branch.
- **Process:**
  1. **Build:** Compiles the .NET code to ensure no syntax errors.
  2. **Test:** Runs the full **xUnit** test suite.
  3. **Deploy:** If (and only if) tests pass, the code is automatically deployed to **AWS Lightsail**.

---

## 🚀 Getting Started

You can run this project either **Locally** (using the .NET CLI) or inside a **Docker Container**.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

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

This project includes a `Dockerfile` and `docker-compose.yml` for containerized deployment.

#### 1. Run with Docker Compose

```bash
docker-compose -f docker-compose.local.yml up -d
```

This will start:
- **Backend API** on port `5000`
- **SQL Server** on port `15000`
- **Frontend** on port `4200`

#### 2. Access the API

- **Swagger UI:** `http://localhost:5000/api/swagger/index.html`

---

## 🧪 Running Tests

This project enforces code quality with a full Unit Test suite covering Services, Controllers, and Auth logic.

To execute the tests:

```bash
cd POS.Tests
dotnet test
```

---

## 🔑 API Documentation (Swagger)

The Swagger UI provides interactive documentation to test endpoints.

- **Live (AWS Lightsail):** `https://pos.thisara.dev/api/swagger/index.html`
- **Local:** `http://localhost:5050/api/swagger/index.html`
- **Docker:** `http://localhost:5000/api/swagger/index.html`

### Authentication Flow

Most endpoints are protected (🔒). To test them:

1. Use `/api/Auth/login` to get an **AccessToken**.
2. Click **Authorize** at the top right of Swagger.
3. Enter `Bearer <your-token>`.

#### 🔐 Demo Credentials

The application database is seeded with the following default accounts for testing purposes:

| Role | Username | Password | Access Level |
| --- | --- | --- | --- |
| **Super Admin** | `admin@pos.local` | `Admin@1234!` | Full System Access (All Branches) |
| **Admin** | *Create via Super Admin* | *Set during creation* | Branch-specific Management |
| **Manager** | *Create via Super Admin* | *Set during creation* | Inventory Management & Reports |
| **Cashier** | *Create via Super Admin* | *Set during creation* | Sales & Order Processing |

### Roles

- **Super Admin:** Full access to all branches, user management, and system settings.
- **Admin:** Manage users, inventory, and sales within their assigned branch.
- **Manager:** Manage inventory and view reports within their assigned branch.
- **Cashier:** Create orders and process sales within their assigned branch.

---

## 📁 Project Structure

```
POS_System/
├── ApplicationServices/     # Business logic layer (Services)
├── Configurations/          # App configuration classes
├── Controllers/             # API endpoints
├── Data/                    # DbContext and migrations
├── Extensions/              # Extension methods (DI, Swagger, etc.)
├── Helpers/                 # Utility classes
├── Middlewares/             # Custom middleware (BranchActive, ExceptionHandling)
├── Models/                  # Domain entities, DTOs, Identity models
├── Repositories/            # Data access layer
├── POS.Tests/               # Unit tests (xUnit + Moq)
└── Program.cs               # Application entry point
```

---

## 👤 Developer

- **Author:** RKD Thisara Sandeep
- **GitHub:** [github.com/ThisaraRajapakshe](https://github.com/ThisaraRajapakshe)
- **Portfolio:** [thisara.dev](https://thisara.dev)
- **LinkedIn:** [linkedin.com/in/thisara-rajapakshe](https://linkedin.com/in/thisara-rajapakshe)

---

### 📄 License

This project is open-source and available under the MIT License.

---

**⭐ If you find this project useful, please consider giving it a star!**
```

---
