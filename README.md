# Aplicație Gestionare Garanții

Repozitoriu pentru microserviciile aplicației de gestionare a garanțiilor produselor.

## Structura Proiectului

```
licenta/
├── UserManagement/          # Microserviciu User Management
├── ProductCatalog/          # Microserviciu Product Catalog
│   ├── ProductCatalog.sln
│   ├── ProductCatalog.Domain/
│   ├── ProductCatalog.Service/
│   ├── ProductCatalog.Infrastructure/
│   └── ProductCatalog.Controller/
├── README.md
└── (viitoare microservicii...)
```

## Microservicii

### User Management

Microserviciu backend pentru gestionarea utilizatorilor, profilurilor, abonamentelor și partajării în familie (Family Share). DDD, Clean Architecture, Entity Framework Core.

**Rulare**: `cd UserManagement` apoi `dotnet run --project UserManagement.Controller`  
**Swagger**: http://localhost:5139/swagger (portul poate varia)

### Product Catalog

Microserviciu backend pentru gestionarea produselor și categoriilor, implementat folosind .NET (C#) și respectând principiile Domain-Driven Design (DDD) și Clean Architecture.

#### Structură internă

```
ProductCatalog/
├── ProductCatalog.Domain/          # Stratul Domain (Entities)
│   └── Entities/
│       ├── Product.cs
│       ├── Category.cs
│       └── ProductStatus.cs
│
├── ProductCatalog.Service/         # Stratul Service (Service + Repository Contracts)
│   ├── Repository/
│   │   ├── IProductRepository.cs
│   │   └── ICategoryRepository.cs
│   └── Service/
│       ├── IProductCatalogService.cs
│       └── ProductCatalogService.cs
│
├── ProductCatalog.Infrastructure/  # Stratul Infrastructure (EF Core Implementation)
│   ├── Data/
│   │   └── ProductCatalogDbContext.cs
│   └── Repository/
│       ├── ProductRepository.cs
│       └── CategoryRepository.cs
│
└── ProductCatalog.Controller/       # Stratul Controller (Controllers)
    ├── Controllers/
    │   └── ProductController.cs
    ├── DTOs/
    │   ├── ProductDto.cs
    │   ├── CreateProductRequest.cs
    │   └── UpdateProductRequest.cs
    └── appsettings.json
```

##  Direcția Dependențelor

Arhitectura respectă următoarea direcție a dependențelor:

- **Controller** → **Service**
- **Service** → **Domain**
- **Service** → **Repository**
- **Domain** nu depinde de niciun alt strat

##  Straturi

### Domain (Entities)

Contine entitățile de domeniu:

- **Product**: Reprezintă un produs cu proprietăți (ProductId, Name, Brand, Model, CategoryId, Status) și metode de domeniu (UpdateDetails, ChangeCategory, Deactivate)
- **Category**: Reprezintă o categorie cu proprietăți (CategoryId, Name, Description) și metode de domeniu (Rename)
- **ProductStatus**: Enum cu valori (Active, Inactive, Archived)

### Repository (Contracts)

Definește interfețele pentru accesul la date:

- **IProductRepository**: Contract pentru operațiile pe produse (GetById, GetAll, Add, Update, Delete)
- **ICategoryRepository**: Contract pentru operațiile pe categorii (GetById, GetAll, Add)

**Notă**: Implementările repository-urilor folosesc Entity Framework Core cu SQL Server.

### Service

Orchestrează cazurile de utilizare:

- **IProductCatalogService**: Interfața serviciului
- **ProductCatalogService**: Implementarea serviciului care coordonează operațiile între repository-uri și entitățile de domeniu

### Controller

Expune API-ul REST:

- **ProductController**: Endpoint-uri REST pentru:
  - `POST /api/product` - Creare produs
  - `PUT /api/product/{id}` - Actualizare produs
  - `GET /api/product` - Obținere toate produsele
  - `GET /api/product/{id}` - Obținere produs după ID

##  Rulare

### Cerințe

- .NET 9.0 SDK

### Comenzi

```bash
# Din directorul licenta
cd ProductCatalog

# Restaurare pachete
dotnet restore ProductCatalog.sln

# Compilare
dotnet build ProductCatalog.sln

# Rulare
dotnet run --project ProductCatalog.Controller
```

API-ul va fi disponibil la:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

Swagger UI este disponibil la: `https://localhost:5001/swagger` (în modul Development)

## 📝 Baza de Date

Microserviciul folosește **Entity Framework Core** cu **SQL Server** pentru persistența datelor.

### Configurare Baza de Date

Connection string-ul este configurat în `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ProductCatalogDb": "Server=(localdb)\\mssqllocaldb;Database=ProductCatalogDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Migrații

Pentru a crea sau actualiza baza de date, folosește:

```bash
# Din directorul ProductCatalog
cd ProductCatalog

dotnet ef migrations add MigrationName --project ProductCatalog.Infrastructure --startup-project ProductCatalog.Controller
dotnet ef database update --project ProductCatalog.Infrastructure --startup-project ProductCatalog.Controller
```

Baza de date `ProductCatalogDb` va fi creată automat la prima rulare a aplicației dacă migrațiile au fost aplicate.

## Testare API

### Creare produs

```http
POST /api/product
Content-Type: application/json

{
  "name": "Laptop Dell XPS 15",
  "brand": "Dell",
  "model": "XPS 15",
  "categoryId": "<category-guid>"
}
```

### Actualizare produs

```http
PUT /api/product/{productId}
Content-Type: application/json

{
  "name": "Laptop Dell XPS 15 Updated",
  "brand": "Dell",
  "model": "XPS 15 2024"
}
```

### Obținere toate produsele

```http
GET /api/product
```

### Obținere produs după ID

```http
GET /api/product/{productId}
```

##  Principii Arhitecturale

- **Domain-Driven Design (DDD)**: Entitățile de domeniu conțin logica de business
- **Clean Architecture**: Separarea clară a responsabilităților și direcția dependențelor
- **Dependency Injection**: Toate dependențele sunt injectate prin constructor
- **Repository Pattern**: Abstrage accesul la date
- **Service Layer**: Orchestrează cazurile de utilizare
