# README_CONTEXT

Acest fișier descrie starea curentă a proiectului pentru un alt asistent AI care va continua implementarea. Informațiile de mai jos sunt extrase din codul existent și din configurațiile prezente în repository. Unde ceva nu este implementat sau nu există în cod, este menționat explicit.

## 1. Prezentare generală a proiectului

### Ce face aplicația

Aplicația este o platformă pentru gestionarea garanțiilor produselor. Utilizatorii pot:

- să se înregistreze și să se autentifice
- să își gestioneze profilul
- să administreze produse și categorii
- să creeze și să actualizeze garanții
- să deschidă claim-uri pentru garanții
- să consulte notificări și să le marcheze ca citite
- să genereze și să exporte rapoarte
- să încarce documente (facturi/bonuri), să le analizeze prin OCR și să transforme informația detectată în produse și garanții

### Scopul principal al aplicației

Scopul principal este centralizarea ciclului de viață al garanțiilor produselor într-o aplicație unificată, cu suport pentru documente justificative și fluxuri de administrare a produselor, garanțiilor și notificărilor.

### Rolurile principale ale utilizatorilor

Din frontend și backend rezultă explicit doar rolul de utilizator autentificat. În token-urile de autentificare există suport pentru roluri (`role` / `roles`), însă în frontend nu există în acest moment ecrane sau garduri dedicate pentru roluri administrative separate.

Pe baza funcționalităților implementate se disting practic:

- utilizator autentificat
- utilizator care își administrează propriile produse, garanții și documente
- utilizator care poate partaja acces prin family sharing

Nu există în cod un panou de administrare separat sau un rol `admin` cu UI dedicat.

## 2. Stack tehnologic

### Frontend

- Angular 20
- Angular Router
- Angular Signals
- Angular Reactive Forms
- Angular Material
- RxJS

### Backend

- .NET 9
- ASP.NET Core Web API
- microservicii separate pe soluții distincte
- API Gateway cu YARP Reverse Proxy

### Baza de date

Din fișierele `appsettings.json` existente, serviciile folosesc SQL Server LocalDB:

- `ProductCatalogDb`
- `UserManagementDb`
- `IdentityManagementDb`
- `NotificationManagementDb`
- `WarrantyManagementDb`
- `DocumentAnalysisDb`

Pentru `ReportsManagement` nu există o bază de date proprie în `appsettings.json`; serviciul consumă date din alte microservicii.

### ORM

- Entity Framework Core
- `IdentityManagement` folosește și ASP.NET Core Identity (`IdentityDbContext`)

### Metoda de autentificare

Autentificarea este implementată cu:

- JWT access token
- refresh token
- endpoint-uri de `login`, `register`, `refresh`, `logout`, `me`

Frontendul are:

- interceptor pentru atașarea token-ului
- refresh automat la `401`
- restore de sesiune

### Docker / infrastructură

Nu există în repository:

- `docker-compose.yml`
- `docker-compose.yaml`
- `Dockerfile`

Prin urmare, în starea actuală proiectul este gândit să ruleze local, prin procese separate `dotnet run` și `npm start`.

## 3. Arhitectură

### Structura folderelor

```text
licenta/
├── ApiGateway/
├── DocumentAnalysis/
├── IdentityManagement/
├── NotificationManagement/
├── ProductCatalog/
├── ReportsManagement/
├── UserManagement/
├── WarrantyManagement/
├── frontend/
├── README.md
└── README_CONTEXT.md
```

### Straturi principale

Fiecare microserviciu .NET este organizat, în linii mari, pe straturi de tip Clean Architecture / DDD:

- `*.Domain`
- `*.Service`
- `*.Infrastructure`
- `*.Controller`

### Module / servicii principale

- `ApiGateway`
  - rutare externă către microservicii prin YARP
- `IdentityManagement`
  - autentificare și token-uri
- `UserManagement`
  - profil utilizator, subscription, family sharing
- `ProductCatalog`
  - produse și categorii
- `WarrantyManagement`
  - garanții și claim-uri
- `NotificationManagement`
  - notificări
- `DocumentAnalysis`
  - documente analizate, OCR, extragere de metadate
- `ReportsManagement`
  - rapoarte și export
- `frontend`
  - interfața utilizatorului

### Pattern-uri și abordări importante folosite

- Clean Architecture
- Domain-Driven Design
- Repository pattern
- Service layer
- Dependency Injection
- API Gateway pattern
- Signals pentru stare în Angular

### Cum este organizat backendul

Backendul este împărțit în microservicii independente. Fiecare serviciu expune propriile controllere și propriul model de domeniu. Gateway-ul expune o rută externă pentru fiecare microserviciu:

- `/identity`
- `/users`
- `/products`
- `/warranties`
- `/notifications`
- `/documents`
- `/reports`

`ReportsManagement` și `WarrantyManagement` consumă și ele date din alte microservicii prin clienți HTTP din `Infrastructure`.

## 4. Funcționalități implementate

### Ce funcționează deja

Pe baza codului frontend și backend existent, funcționează la nivel de implementare:

- autentificare cu login / register / refresh / logout / me
- restore sesiune în frontend
- dashboard agregat
- profile page
- subscriptions page
- family sharing page
- produse CRUD
- categorii CRUD
- garanții CRUD
- claim-uri pe garanții
- notificări cu filtru și `mark as read`
- rapoarte și export
- upload document
- analiză document prin `DocumentAnalysis`
- listare documente analizate
- flux ghidat în frontend pentru:
  - confirmarea produselor detectate din document
  - adăugarea acestora în catalog
  - crearea garanțiilor pentru produsele adăugate

### Endpoint-uri existente

#### IdentityManagement

- `POST /api/Auth/register`
- `POST /api/Auth/login`
- `POST /api/Auth/refresh`
- `POST /api/Auth/logout`
- `GET /api/Auth/me`

#### UserManagement

- `POST /api/user`
- `GET /api/user/{id}`
- `GET /api/user`
- `PUT /api/user/{id}`
- `DELETE /api/user/{id}`

- `POST /api/subscription`
- `GET /api/subscription/{id}`
- `GET /api/subscription/user/{userId}`
- `GET /api/subscription`
- `PUT /api/subscription/{id}`
- `DELETE /api/subscription/{id}`

- `POST /api/familyshare`
- `GET /api/familyshare/{id}`
- `GET /api/familyshare`
- `GET /api/familyshare/owner/{ownerUserId}`
- `GET /api/familyshare/member/{memberUserId}`
- `PUT /api/familyshare/{id}`
- `DELETE /api/familyshare/{id}`

#### ProductCatalog

- `POST /api/product`
- `PUT /api/product/{id}`
- `GET /api/product`
- `GET /api/product/{id}`
- `DELETE /api/product/{id}`

- `POST /api/category`
- `PUT /api/category/{id}`
- `GET /api/category`
- `GET /api/category/{id}`
- `DELETE /api/category/{id}`

#### WarrantyManagement

- `POST /api/warranty`
- `POST /api/warranty/from-document`
- `POST /api/warranty/from-analyzed-document/{documentId}`
- `GET /api/warranty/{id}`
- `GET /api/warranty`
- `GET /api/warranty/user/{userId}`
- `GET /api/warranty/product/{productId}`
- `PUT /api/warranty/{id}`
- `POST /api/warranty/{id}/refresh-status`
- `DELETE /api/warranty/{id}`

- `POST /api/claim`
- `GET /api/claim/{id}`
- `GET /api/claim/warranty/{warrantyId}`
- `POST /api/claim/{id}/close`

#### NotificationManagement

- `POST /api/notification`
- `GET /api/notification/{id}`
- `GET /api/notification`
- `GET /api/notification/user/{userId}`
- `GET /api/notification/user/{userId}/unread`
- `POST /api/notification/{id}/mark-sent`
- `POST /api/notification/{id}/mark-read`
- `POST /api/notification/{id}/mark-failed`
- `DELETE /api/notification/{id}`

#### DocumentAnalysis

- `POST /api/document/analyze`
- `GET /api/document/{id}`
- `GET /api/document`
- `POST /api/document/{id}/warranty-draft`

#### ReportsManagement

- `GET /api/reports/portfolio`
- `GET /api/reports/portfolio/export`
- `GET /api/reports/expiring-warranties`
- `GET /api/reports/expiring-warranties/export`

### Pagini / componente existente în frontend

Rutele principale din frontend sunt:

- `/dashboard`
- `/profile`
- `/subscriptions`
- `/family-sharing`
- `/products`
- `/categories`
- `/warranties`
- `/notifications`
- `/documents`
- `/reports`
- `/auth/login`
- `/auth/register`

Componente importante:

- `AppShellComponent`
- `DashboardPageComponent`
- `ProfilePageComponent`
- `SubscriptionsPageComponent`
- `FamilySharingPageComponent`
- `ProductsPageComponent`
- `CategoriesPageComponent`
- `WarrantiesPageComponent`
- `NotificationsPageComponent`
- `DocumentsPageComponent`
- `ReportsPageComponent`
- `LoginPageComponent`
- `RegisterPageComponent`

### Entități / tabele existente

Din codul curent se pot identifica următoarele entități:

#### IdentityManagement

- `ApplicationUser`
- `RefreshToken`

#### UserManagement

- `User`
- `UserProfile`
- `Subscription`
- `FamilyShare`

#### ProductCatalog

- `Product`
- `Category`

#### WarrantyManagement

- `Warranty`
- `Claim`

#### NotificationManagement

- `Notification`

#### DocumentAnalysis

- `AnalyzedDocument`

### Migrații existente

În repository există explicit doar migrații pentru `ProductCatalog`:

- `ProductCatalog/ProductCatalog.Infrastructure/Migrations/20260115132617_InitialCreate.cs`
- `ProductCatalog/ProductCatalog.Infrastructure/Migrations/20260115132617_InitialCreate.Designer.cs`
- `ProductCatalog/ProductCatalog.Infrastructure/Migrations/ProductCatalogDbContextModelSnapshot.cs`

Nu au fost identificate în repository migrații explicite pentru celelalte microservicii. Este posibil ca acestea să folosească `EnsureCreated`, migrații necomise sau alt flux intern, dar acest lucru nu trebuie presupus fără verificare suplimentară.

## 5. Părți nefinalizate sau parțial implementate

### Ce lipsește

- Docker / Docker Compose nu este implementat
- nu există testare automată vizibilă pentru fluxurile de business
- nu există panou admin separat
- nu există gestionare avansată a rolurilor în frontend

### Ce este parțial implementat

- fluxul de documente detectează produse și permite confirmarea lor, dar detecția produselor este euristică și bazată pe `lineItems` și text extras
- backendul nu modelează explicit mai multe garanții per line item dintr-un document; frontendul orchestrează manual acest flux
- README-ul proiectului general a fost actualizat, dar documentația de infrastructură este încă limitată

### Ce mai trebuie conectat sau validat

- testare reală end-to-end cu toate microserviciile pornite simultan
- validare că toate serviciile folosesc corect aceleași identificatoare de utilizator și produse în fluxurile cross-service
- validare completă pentru `DocumentAnalysis -> ProductCatalog -> WarrantyManagement`

## 6. Fișiere importante

### Root

- [README.md](README.md)
  Documentația generală a proiectului
- [README_CONTEXT.md](README_CONTEXT.md)
  Context tehnic pentru continuarea implementării

### Gateway

- [ApiGateway/ApiGateway/appsettings.json](ApiGateway/ApiGateway/appsettings.json)
  Configurația tuturor rutelor și a porturilor pentru microservicii

### Frontend

- [frontend/src/app/app.routes.ts](frontend/src/app/app.routes.ts)
  Toate rutele principale ale aplicației
- [frontend/src/app/core/auth/auth.interceptor.ts](frontend/src/app/core/auth/auth.interceptor.ts)
  Atașează token-ul și face refresh la `401`
- [frontend/src/app/features/auth/view-models/auth.view-model.ts](frontend/src/app/features/auth/view-models/auth.view-model.ts)
  Sesiunea utilizatorului, restore, refresh și logout
- [frontend/src/app/features/dashboard/data/dashboard-api.service.ts](frontend/src/app/features/dashboard/data/dashboard-api.service.ts)
  Agregă apelurile API folosite de dashboard și alte feature-uri
- [frontend/src/app/features/account/data/account-api.service.ts](frontend/src/app/features/account/data/account-api.service.ts)
  API client pentru profile, subscription și family sharing
- [frontend/src/app/features/documents/data/documents-api.service.ts](frontend/src/app/features/documents/data/documents-api.service.ts)
  API client pentru document analysis și creare de garanții din documente
- [frontend/src/app/features/documents/presentation/documents-page/documents-page.component.ts](frontend/src/app/features/documents/presentation/documents-page/documents-page.component.ts)
  Fluxul principal de documente
- [frontend/src/app/features/notifications/presentation/notifications-page/notifications-page.component.ts](frontend/src/app/features/notifications/presentation/notifications-page/notifications-page.component.ts)
  Notificări, filtre și `mark as read`
- [frontend/proxy.conf.json](frontend/proxy.conf.json)
  Proxy local spre gateway în timpul dezvoltării

### Backend

- `*/Controller/Controllers/*.cs`
  Endpoint-urile REST ale fiecărui microserviciu
- `*/Infrastructure/*DbContext.cs`
  Configurația bazei de date per serviciu
- `*/Service/Services/*.cs`
  Logica de business și use-case orchestration

## 7. Configurare

### Variabile de mediu

Nu există în repository un fișier `.env` sau o configurare centralizată de variabile de mediu. Configurațiile active sunt în `appsettings.json`.

### Connection strings

Din `appsettings.json`:

- `ProductCatalogDb` -> `Server=(localdb)\mssqllocaldb;Database=ProductCatalogDb;...`
- `UserManagementDb` -> `Server=(localdb)\mssqllocaldb;Database=UserManagementDb;...`
- `IdentityManagementDb` -> `Server=(localdb)\mssqllocaldb;Database=IdentityManagementDb;...`
- `NotificationManagementDb` -> `Server=(localdb)\mssqllocaldb;Database=NotificationManagementDb;...`
- `WarrantyManagementDb` -> `Server=(localdb)\mssqllocaldb;Database=WarrantyManagementDb;...`
- `DocumentAnalysisDb` -> `Server=(localdb)\mssqllocaldb;Database=DocumentAnalysisDb;...`

### Servicii Docker Compose

Nu există servicii Docker Compose în codul curent.

### Porturi folosite

Conform gateway-ului și rulărilor observate:

- `ApiGateway` -> `5005`
- `IdentityManagement` -> `5119`
- `UserManagement` -> `5139`
- `ProductCatalog` -> `5085`
- `NotificationManagement` -> `5160`
- `ReportsManagement` -> `5270`
- `WarrantyManagement` -> `5281`
- `DocumentAnalysis` -> `5291`
- `frontend` Angular dev server -> implicit `4200` dacă nu este schimbat

## 8. Cum se rulează proiectul

### Comenzi pentru backend

Din rădăcina proiectului:

```powershell
dotnet run --project IdentityManagement\IdentityManagement.Controller
dotnet run --project UserManagement\UserManagement.Controller
dotnet run --project ProductCatalog\ProductCatalog.Controller
dotnet run --project NotificationManagement\NotificationManagement.Controller
dotnet run --project ReportsManagement\ReportsManagement.Controller
dotnet run --project WarrantyManagement\WarrantyManagement.Controller
dotnet run --project DocumentAnalysis\DocumentAnalysis.Controller
dotnet run --project ApiGateway\ApiGateway
```

Microserviciile trebuie pornite în paralel pentru ca frontendul să funcționeze complet.

### Comenzi pentru frontend

```powershell
cd frontend
npm install
npm start
```

Build:

```powershell
cd frontend
npm run build
```

### Comenzi Docker

Nu există comenzi Docker valide în starea actuală a proiectului, deoarece nu există fișiere Docker în repository.

### Comenzi pentru migrații

În mod cert există migrații pentru `ProductCatalog`:

```powershell
cd ProductCatalog
dotnet ef database update --project ProductCatalog.Infrastructure --startup-project ProductCatalog.Controller
```

Pentru celelalte servicii, fișierele de migrare nu sunt prezente explicit în repository și trebuie verificat separat modul în care sunt inițializate bazele de date.

## 9. Probleme cunoscute

### Erori / comportamente cunoscute

- `DocumentAnalysis` afișează warning EF Core pentru proprietatea decimal `TotalAmount` dacă nu este specificată precisia în model
- `ApiGateway` poate loga warning `Failed to determine the https port for redirect`
- fluxurile end-to-end depind de pornirea simultană a tuturor microserviciilor relevante

### Părți fragile / datorie tehnică

- detecția produselor din document este euristică și poate necesita rafinare
- doar `ProductCatalog` are migrații prezente explicit în repository
- lipsesc fișiere de infrastructură pentru deploy (`Dockerfile`, `docker-compose`)
- o parte din interfețe și texte au fost iterativ ajustate în frontend și merită o trecere finală de UX și accesibilitate

### Zone de refactorizare

- unificarea modului de inițializare a bazelor de date între toate microserviciile
- definirea clară a contractelor cross-service pentru document -> produs -> garanție
- extragerea unui nivel mai clar pentru orchestration în fluxul de documente, dacă logica devine mai complexă

## 10. Pași recomandați în continuare

Ordinea recomandată:

1. Validare end-to-end cu toate microserviciile pornite simultan
   - login
   - dashboard
   - products / categories
   - document upload
   - confirmare produse detectate
   - creare garanții
   - notificări `mark as read`

2. Verificare și consolidare baze de date
   - verifica dacă toate serviciile folosesc migrații sau alt mecanism de creare schema
   - adaugă migrații lipsă unde este cazul

3. Îmbunătățirea fluxului DocumentAnalysis
   - rafinează detecția produselor
   - gestionează mai bine documentele cu mai multe produse
   - separă mai clar partea de preview de partea de creare efectivă

4. Introducerea infrastructurii de rulare
   - adaugă `Dockerfile` pentru fiecare serviciu
   - adaugă `docker-compose.yml`
   - descrie ordinea de pornire în medii locale și de demo

5. Adăugare testare
   - teste unitare pentru servicii
   - teste de integrare pentru controllere importante
   - eventual smoke tests pentru frontend

6. Curățare și standardizare
   - uniformizare naming între proiecte
   - revizuire warning-uri EF Core și model precision
   - clarificare roluri și politici de autorizare în frontend și backend
