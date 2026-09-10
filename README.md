# SmartWarranty

SmartWarranty is a warranty management platform that brings product records, purchase documents, warranty coverage, and claims into one application. It combines an Angular frontend with a .NET microservices backend and a YARP API gateway.

## Capabilities

- **Account management:** registration, authentication, token refresh, user profiles, subscriptions, and family sharing.
- **Product catalog:** create and maintain products and categories.
- **Warranty tracking:** manage coverage periods, review warranty status, and submit claims.
- **Document analysis:** extract text from invoices and receipts, identify purchase details, and review detected products before creating warranties.
- **Notifications:** view updates, filter unread notifications, and mark messages as read.
- **Reporting:** preview portfolio and expiring warranty reports and export them as PDF or Excel files.
- **Dashboard:** review products, warranties, notifications, and account information from a central view.

## Architecture

The frontend accesses backend services through the API gateway. Services separate domain models, application logic, infrastructure, and HTTP controllers into dedicated projects.

| Component | Responsibility | Gateway prefix |
| --- | --- | --- |
| `ApiGateway` | Request routing with YARP | Entry point |
| `IdentityManagement` | Authentication, roles, and session management | `/identity` |
| `UserManagement` | Profiles, subscriptions, and family sharing | `/users` |
| `ProductCatalog` | Products and categories | `/products` |
| `WarrantyManagement` | Warranty coverage and claims | `/warranties` |
| `NotificationManagement` | Notifications and read status | `/notifications` |
| `DocumentAnalysis` | Document processing, OCR, and warranty drafts | `/documents` |
| `ReportsManagement` | Portfolio and warranty reporting | `/reports` |
| `frontend` | Angular user interface | Web application |

### Technology stack

- .NET 9 and ASP.NET Core
- Entity Framework Core with SQL Server
- ASP.NET Core Identity and JWT authentication
- YARP reverse proxy
- Angular 20, Angular Material, Signals, and Reactive Forms
- Tesseract OCR with Romanian and English language support
- Docker Compose for local orchestration

## Document processing workflow

1. Upload an invoice or receipt.
2. Extract embedded text or use OCR where required.
3. Review detected purchase information and product candidates.
4. Correct product details and add selected items to the catalog.
5. Create warranties for the selected products using the analyzed document.

Document interpretation uses heuristics. Extraction accuracy depends on document quality and layout, so users review the detected information before saving it. Multiple products can reference the same analyzed document; the backend does not expose a separate warranty endpoint for each extracted line item.

## Getting started

### Run with Docker Compose

Install Docker with support for Linux containers and Docker Compose, then run from the repository root:

```sh
docker compose up --build -d
```

| Endpoint | Address |
| --- | --- |
| Web application | http://localhost:4200 |
| API gateway | http://localhost:5005 |
| SQL Server | localhost,14333 |

The Compose stack includes SQL Server, the backend services, the gateway, and the frontend. SQL Server data is persisted in a named volume. The document analysis image includes Tesseract and its language data.

Create an account through the registration page. Startup initializes database structures and application roles without creating predefined accounts or sample business records. Existing database contents are preserved.

View service status and logs:

```sh
docker compose ps
docker compose logs -f
```

Stop the stack:

```sh
docker compose down
```

The supplied Compose configuration is intended for local development. Configure database credentials and the JWT signing key for your deployment environment before using it outside a local setup.

### Run locally

Prerequisites:

- .NET 9 SDK
- Node.js and npm compatible with Angular 20
- SQL Server or SQL Server LocalDB, matching the service connection strings
- Tesseract with English and Romanian language data for OCR

Configure connection strings, JWT settings, external service URLs, and Tesseract paths in the relevant service configuration or environment variables. Environment variable keys use double underscores for nested settings, such as `ConnectionStrings__UserManagementDb` and `Tesseract__ExecutablePath`.

Start each backend service in a separate terminal from the repository root:

```sh
dotnet run --project IdentityManagement/IdentityManagement.Controller
dotnet run --project UserManagement/UserManagement.Controller
dotnet run --project ProductCatalog/ProductCatalog.Controller
dotnet run --project WarrantyManagement/WarrantyManagement.Controller
dotnet run --project NotificationManagement/NotificationManagement.Controller
dotnet run --project DocumentAnalysis/DocumentAnalysis.Controller
dotnet run --project ReportsManagement/ReportsManagement.Controller
dotnet run --project ApiGateway/ApiGateway
```

Start the frontend:

```sh
cd frontend
npm ci
npm start
```

Open http://localhost:4200. The frontend development proxy forwards API requests to the gateway at http://localhost:5005. Keep all backend services running to support the full application workflow.

## Build

Build a backend solution, for example:

```sh
dotnet build DocumentAnalysis/DocumentAnalysis.sln
```

Each backend component has its own solution file. Build the Angular application with:

```sh
cd frontend
npm run build
```

## Project references

- [Gateway configuration](ApiGateway/ApiGateway/appsettings.json)
- [Container orchestration](docker-compose.yml)
- [Frontend dependencies and scripts](frontend/package.json)
- [Frontend routes](frontend/src/app/app.routes.ts)
- [Frontend development proxy](frontend/proxy.conf.json)
- [Document analysis service](DocumentAnalysis/DocumentAnalysis.Service/Services/DocumentAnalysisService.cs)
