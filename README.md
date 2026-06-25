# SmartWarranty

Aplicatie de gestionare a garantiilor construita pe o arhitectura de microservicii .NET, cu frontend Angular si API Gateway YARP. Proiectul acopera autentificare, profil utilizator, produse, categorii, garantii, notificari, rapoarte si analiza de documente pentru creare de garantii din facturi sau bonuri.

## Scop

Aplicatia permite unui utilizator sa:

- isi creeze cont si sa se autentifice
- isi administreze profilul, abonamentul si family sharing
- isi gestioneze produsele si categoriile
- creeze si actualizeze garantii
- deschida claim-uri pentru garantii
- vada si marcheze notificari ca citite
- exporte rapoarte
- incarce un document, sa il analizeze cu OCR si sa creeze produse si garantii pe baza informatiei detectate

## Arhitectura

Solutia foloseste o arhitectura de tip microservices + API Gateway:

- `ApiGateway`
  Rol: un singur punct de intrare pentru frontend
  Tehnologie: YARP Reverse Proxy
- `IdentityManagement`
  Rol: autentificare, token-uri, refresh token, logout, endpoint `me`
- `UserManagement`
  Rol: user profile, subscriptions, family sharing
- `ProductCatalog`
  Rol: produse si categorii
- `WarrantyManagement`
  Rol: garantii si claim-uri
- `NotificationManagement`
  Rol: notificari si statusurile lor
- `DocumentAnalysis`
  Rol: upload document, extragere text, OCR, extragere metadate, draft garantie
- `ReportsManagement`
  Rol: rapoarte de portofoliu si garantii care expira
- `frontend`
  Rol: UI Angular pentru toate fluxurile functionale

## Structura repository

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
└── README.md
```

Fiecare microserviciu .NET este organizat pe straturi apropiate de Clean Architecture / DDD:

- `*.Domain`
- `*.Service`
- `*.Infrastructure`
- `*.Controller`

## Gateway si rutare

Frontendul nu consuma direct fiecare microserviciu, ci merge prin `ApiGateway`, configurat in:

- [ApiGateway/ApiGateway/appsettings.json](ApiGateway/ApiGateway/appsettings.json)

Rutele publice expuse de gateway sunt:

- `/identity` -> `IdentityManagement` la `http://localhost:5119`
- `/users` -> `UserManagement` la `http://localhost:5139`
- `/products` -> `ProductCatalog` la `http://localhost:5085`
- `/warranties` -> `WarrantyManagement` la `http://localhost:5281`
- `/notifications` -> `NotificationManagement` la `http://localhost:5160`
- `/documents` -> `DocumentAnalysis` la `http://localhost:5291`
- `/reports` -> `ReportsManagement` la `http://localhost:5270`

Gateway-ul ruleaza implicit la:

- `http://localhost:5005`

## Frontend

Frontendul este o aplicatie Angular 20 cu Angular Material.

Fisiere importante:

- [frontend/package.json](frontend/package.json)
- [frontend/src/app/app.routes.ts](frontend/src/app/app.routes.ts)
- [frontend/src/app/core/shell/app-shell.component.html](frontend/src/app/core/shell/app-shell.component.html)

### Tehnologii frontend

- Angular 20
- Angular Router
- Angular Signals
- Angular Reactive Forms
- Angular Material
- HttpClient + interceptor pentru autentificare

### Structura frontend

Zone principale:

- `core`
  auth, config, shell
- `features/auth`
  login, register, auth session state
- `features/dashboard`
  dashboard agregat din mai multe microservicii
- `features/account`
  profile, subscriptions, family sharing
- `features/products`
  CRUD produse
- `features/categories`
  CRUD categorii
- `features/warranties`
  CRUD garantii + claim-uri
- `features/notifications`
  listare notificari, filtre, mark as read
- `features/reports`
  previzualizare si export rapoarte
- `features/documents`
  upload, OCR, produse detectate, creare garantii din document

## Functionalitati implementate

### 1. Autentificare si sesiune

Implementare in:

- [frontend/src/app/features/auth/data/auth-api.service.ts](frontend/src/app/features/auth/data/auth-api.service.ts)
- [frontend/src/app/features/auth/view-models/auth.view-model.ts](frontend/src/app/features/auth/view-models/auth.view-model.ts)
- [frontend/src/app/core/auth/auth.interceptor.ts](frontend/src/app/core/auth/auth.interceptor.ts)

Ce face:

- login
- register
- restaurare sesiune la reload
- apel `me` la pornire
- refresh token automat la `401`
- refresh programat inainte de expirare
- logout local + logout in backend

Date salvate local:

- access token
- refresh token
- data expirarii access token
- user curent

### 2. Dashboard

Implementare in:

- [frontend/src/app/features/dashboard/data/dashboard-api.service.ts](frontend/src/app/features/dashboard/data/dashboard-api.service.ts)
- [frontend/src/app/features/dashboard/presentation/dashboard-page/dashboard-page.component.ts](frontend/src/app/features/dashboard/presentation/dashboard-page/dashboard-page.component.ts)

Dashboard-ul agrega date din:

- users
- products
- warranties
- notifications
- reports

Afiseaza:

- numar utilizatori
- produse
- garantii active
- garantii care expira in 30 de zile
- notificari necitite
- rapoarte exportabile
- linkuri rapide spre `Profile`, `Subscriptions`, `Family Sharing`, `Documents`

### 3. Profile, subscriptions, family sharing

Implementare in:

- [frontend/src/app/features/account/data/account-api.service.ts](frontend/src/app/features/account/data/account-api.service.ts)
- [frontend/src/app/features/account/presentation/profile-page/profile-page.component.ts](frontend/src/app/features/account/presentation/profile-page/profile-page.component.ts)
- [frontend/src/app/features/account/presentation/subscriptions-page/subscriptions-page.component.ts](frontend/src/app/features/account/presentation/subscriptions-page/subscriptions-page.component.ts)
- [frontend/src/app/features/account/presentation/family-sharing-page/family-sharing-page.component.ts](frontend/src/app/features/account/presentation/family-sharing-page/family-sharing-page.component.ts)

Implementat:

- creare sau actualizare profil utilizator
- vizualizare plan activ si editare subscription
- creare, editare si stergere family shares

### 4. Produse si categorii

Implementare in:

- [frontend/src/app/features/products/presentation/products-page/products-page.component.ts](frontend/src/app/features/products/presentation/products-page/products-page.component.ts)
- [frontend/src/app/features/categories/presentation/categories-page/categories-page.component.ts](frontend/src/app/features/categories/presentation/categories-page/categories-page.component.ts)

Implementat:

- creare produs
- editare produs
- stergere produs
- creare categorie
- editare categorie
- stergere categorie

Legaturi UX:

- din `Products` exista link direct catre `Categories`

### 5. Garantii si claim-uri

Implementare in:

- [frontend/src/app/features/warranties/presentation/warranties-page/warranties-page.component.ts](frontend/src/app/features/warranties/presentation/warranties-page/warranties-page.component.ts)

Implementat:

- creare garantie
- editare garantie
- stergere garantie
- creare claim
- vizualizare claim-uri pe garantie

### 6. Notificari

Implementare in:

- [frontend/src/app/features/notifications/presentation/notifications-page/notifications-page.component.ts](frontend/src/app/features/notifications/presentation/notifications-page/notifications-page.component.ts)
- [frontend/src/app/features/notifications/data/notifications-state.service.ts](frontend/src/app/features/notifications/data/notifications-state.service.ts)

Implementat:

- listare notificari
- filtru `All / Unread`
- `mark as read`
- badge cu numar de notificari necitite in sidebar si topbar
- stare independenta pe fiecare buton `Mark as read`

### 7. Rapoarte

Implementare in:

- [frontend/src/app/features/reports/presentation/reports-page/reports-page.component.ts](frontend/src/app/features/reports/presentation/reports-page/reports-page.component.ts)

Implementat:

- previzualizare raport portofoliu
- previzualizare raport garantii care expira
- export PDF
- export Excel

### 8. Flux documente si OCR

Implementare in:

- [frontend/src/app/features/documents/data/documents-api.service.ts](frontend/src/app/features/documents/data/documents-api.service.ts)
- [frontend/src/app/features/documents/presentation/documents-page/documents-page.component.ts](frontend/src/app/features/documents/presentation/documents-page/documents-page.component.ts)
- [frontend/src/app/features/documents/models/document.models.ts](frontend/src/app/features/documents/models/document.models.ts)

#### Fluxul initial

Frontendul incarca documentul prin:

- `POST /documents/api/document/analyze`

Backendul returneaza:

- `documentId`
- text extras
- merchant
- numar document
- suma
- data
- tip document
- durata garantie detectata
- `lineItems`

#### Fluxul implementat in UI

Dupa analiza:

1. utilizatorul vede documentul in lista de documente analizate
2. aplicatia construieste produse candidate pe baza `lineItems` si a textului extras
3. pentru fiecare produs detectat, utilizatorul poate:
   - confirma ca vrea sa il adauge
   - sari peste el
   - edita `name`, `brand`, `model`, `category`
4. utilizatorul apasa `Adauga produsele selectate`
5. pentru produsele create, utilizatorul poate decide:
   - `Da, garantie`
   - `Nu acum`
6. pentru fiecare garantie se poate seta numarul de luni
7. utilizatorul apasa `Creeaza garantiile selectate`

#### Observatie importanta

Backendul creeaza garantia din:

- `documentId`
- `productId`
- `defaultDurationMonths`

Nu exista in acest moment un endpoint separat per line item. Din acest motiv, frontendul orchestreaza mai multe produse detectate din acelasi document si creeaza garantii separate folosind acelasi document analizat.

## Implementari importante in backend

### DocumentAnalysis.Infrastructure.Tasks

In `DocumentAnalysis` si `IdentityManagement`, folderele si namespace-urile `Infrastructure.Services` au fost redenumite in `Infrastructure.Tasks`, pentru aliniere mai buna a responsabilitatilor interne.

Exemple:

- `DocumentAnalysis.Infrastructure.Tasks`
- `IdentityManagement.Infrastructure.Tasks`

### Endpointuri relevante folosite de frontend

#### IdentityManagement

- `POST /api/Auth/register`
- `POST /api/Auth/login`
- `POST /api/Auth/refresh`
- `POST /api/Auth/logout`
- `GET /api/Auth/me`

#### UserManagement

- `GET /api/user/{id}`
- `PUT /api/user/{id}`
- `POST /api/user`
- `GET /api/subscription/user/{userId}`
- `POST /api/subscription`
- `PUT /api/subscription/{id}`
- `DELETE /api/subscription/{id}`
- `GET /api/familyshare/owner/{ownerUserId}`
- `GET /api/familyshare/member/{memberUserId}`
- `POST /api/familyshare`
- `PUT /api/familyshare/{id}`
- `DELETE /api/familyshare/{id}`

#### ProductCatalog

- `GET /api/product`
- `POST /api/product`
- `PUT /api/product/{id}`
- `DELETE /api/product/{id}`
- `GET /api/category`
- `POST /api/category`
- `PUT /api/category/{id}`
- `DELETE /api/category/{id}`

#### WarrantyManagement

- `GET /api/warranty`
- `GET /api/warranty/user/{userId}`
- `POST /api/warranty`
- `PUT /api/warranty/{id}`
- `DELETE /api/warranty/{id}`
- `POST /api/warranty/from-analyzed-document/{documentId}`
- `GET /api/claim/warranty/{warrantyId}`
- `POST /api/claim`

#### NotificationManagement

- `GET /api/notification/user/{userId}`
- `GET /api/notification/user/{userId}/unread`
- `POST /api/notification/{id}/mark-read`

#### DocumentAnalysis

- `GET /api/document`
- `POST /api/document/analyze`
- `POST /api/document/{id}/warranty-draft`

#### ReportsManagement

- `GET /api/reports/portfolio`
- `GET /api/reports/portfolio/export`
- `GET /api/reports/expiring-warranties`
- `GET /api/reports/expiring-warranties/export`

## Cum rulezi proiectul

### Cerinte

- .NET 9 SDK
- Node.js + npm
- SQL Server / LocalDB, in functie de configurarea fiecarui microserviciu

### 1. Pornire microservicii

Porneste fiecare serviciu separat din root:

```powershell
dotnet run --project IdentityManagement\IdentityManagement.Controller
dotnet run --project UserManagement\UserManagement.Controller
dotnet run --project ProductCatalog\ProductCatalog.Controller
dotnet run --project WarrantyManagement\WarrantyManagement.Controller
dotnet run --project NotificationManagement\NotificationManagement.Controller
dotnet run --project DocumentAnalysis\DocumentAnalysis.Controller
dotnet run --project ReportsManagement\ReportsManagement.Controller
dotnet run --project ApiGateway\ApiGateway
```

Important:

- `DocumentAnalysis` trebuie sa ramana pornit in paralel cu `ApiGateway`
- daca `DocumentAnalysis` este inchis, upload-ul poate merge partial sau lista de documente poate esua

### 2. Pornire frontend

```powershell
cd frontend
npm install
npm start
```

Frontendul ruleaza cu proxy:

- [frontend/proxy.conf.json](frontend/proxy.conf.json)

Proxy-ul trimite toate apelurile `/identity`, `/users`, `/products`, `/warranties`, `/notifications`, `/documents`, `/reports` catre gateway-ul de la `http://localhost:5005`.

### 3. Build frontend

```powershell
cd frontend
npm run build
```

Build-ul a fost verificat si trece.

## Observatii de implementare

- aplicatia foloseste `signals` in frontend pentru stare locala si reactive UI
- pentru modificarile de auth si refresh token s-a mers pe un model simplu, explicit si predictibil
- document flow-ul este intentionat semi-ghidat: utilizatorul confirma produsele detectate in loc sa se creeze totul complet automat
- butoanele de notificari au stare independenta, nu globala
- navigarea interna foloseste `routerLink`

## Limitari actuale

- detectia produselor din document este euristica, nu AI semantic dedicat per produs
- un document poate contine mai multe produse, dar backendul nu modeleaza inca explicit o garantie per line item
- `DocumentAnalysis` poate afisa warning EF Core pentru campurile `decimal` daca nu este definita explicit precizia in model

## Fisiere importante pentru prezentare

- [README.md](README.md)
- [ApiGateway/ApiGateway/appsettings.json](ApiGateway/ApiGateway/appsettings.json)
- [frontend/src/app/app.routes.ts](frontend/src/app/app.routes.ts)
- [frontend/src/app/core/shell/app-shell.component.html](frontend/src/app/core/shell/app-shell.component.html)
- [frontend/src/app/features/auth/view-models/auth.view-model.ts](frontend/src/app/features/auth/view-models/auth.view-model.ts)
- [frontend/src/app/features/dashboard/data/dashboard-api.service.ts](frontend/src/app/features/dashboard/data/dashboard-api.service.ts)
- [frontend/src/app/features/documents/presentation/documents-page/documents-page.component.ts](frontend/src/app/features/documents/presentation/documents-page/documents-page.component.ts)
- [frontend/src/app/features/notifications/presentation/notifications-page/notifications-page.component.ts](frontend/src/app/features/notifications/presentation/notifications-page/notifications-page.component.ts)

## Status

Implementarea acopera atat backend routing + microservices, cat si frontend functional pentru fluxurile principale ale aplicatiei.

Zona cea mai avansata din proiect este acum fluxul:

- upload document
- analiza OCR
- confirmare produse detectate
- adaugare produse in catalog
- creare garantii din document
