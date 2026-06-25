# SmartWarranty Docker demo

Acest setup ruleaza aplicatia SmartWarranty complet in Docker:

- Frontend Angular: `http://localhost:4200`
- API Gateway: `http://localhost:5005`
- SQL Server: container Docker, port host `14333`
- Microserviciile ruleaza intern in reteaua Docker

## Pornire

Din radacina proiectului:

```powershell
docker compose up -d --build
```

Prima pornire poate dura cateva minute, deoarece se descarca imaginea SQL Server si se creeaza bazele de date.

## Verificare

```powershell
docker compose ps
```

Aplicatia se acceseaza din browser:

```text
http://localhost:4200
```

Cont recomandat pentru demo:

```text
Email: ana.popescu@demo.local
Parola: Demo123!
```

Cont admin:

```text
Email: admin@smartwarranty.local
Parola: Admin123!
```

## Oprire

```powershell
docker compose down
```

## Reset complet date demo

Sterge si volumul SQL Server, apoi recreeaza bazele de date si seed-ul:

```powershell
docker compose down -v
docker compose up -d --build
```

## Daca SQL Server nu se descarca

Daca apare o eroare Docker de tip `input/output error` sau `read-only file system`, problema este in Docker Desktop, nu in cod. De obicei se rezolva asa:

1. Inchide Docker Desktop complet.
2. Porneste Docker Desktop din nou.
3. Ruleaza:

```powershell
docker compose pull sqlserver
docker compose up -d
```

## Note

Datele demo sunt create automat de seederele microserviciilor la pornire. Daca baza SQL exista deja in volumul Docker, seederele nu dubleaza datele.
