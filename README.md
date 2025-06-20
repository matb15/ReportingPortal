Documentazione del Progetto
🚀 Tecnologie Utilizzate
Frontend (Blazor WASM - PWA):

Blazor WebAssembly con supporto offline (Progressive Web App)

Leaflet.js (integrazione via JS interop)

JWT per autenticazione custom

Backend (ASP.NET Core):

Appwrite per storage immagini

ASP.NET Core Web API

Entity Framework Core per accesso e migrazioni database

Swagger per documentazione API

Job scheduler interno per notifiche e task ricorrenti

PDF export come byte[] da endpoint API

🖼️ Frontend – Blazor WebAssembly (PWA)
Struttura
Pages/ – Pagine Blazor

Shared/ – Componenti riutilizzabili

Services/ – Servizi per API REST (HttpClient)

wwwroot/ – Manifest e configurazione PWA

Autenticazione Custom (JWT)
JWT salvato nel localStorage

CustomAuthProvider implementa AuthenticationStateProvider

Login → chiamata API backend, salvataggio token

Token incluso in ogni richiesta con HttpClient configurato

Logout → rimozione token, reset stato utente

Integrazione Leaflet.js
Integrazione tramite JavaScript interop

Leaflet.js incluso via wwwroot/js/leaflet.js

Chiamate runtime JS via IJSRuntime

Le richieste ai tile/layer protetti vengono autorizzate via JWT

🔙 Backend – ASP.NET Core
Struttura
Controllers/ – REST API organizzate per ambito (autenticazione, report, mappe)

Services/ – Logica applicativa (notifiche, PDF, Appwrite, etc.)

Services/Jobs/ – Task periodici schedulati (notifiche push/email, controlli automatici)

Controller REST + Swagger
Controller protetti con [Authorize] e JWT bearer

🛠️ Job Scheduler & Task Periodici
Sistema di job interno o Hangfire

Esempi di task:

Check report: dopo 7 giorni, verifica status e invia notifica se necessario

Push Notifications: invio notifiche a utenti mobile/PWA

Email Notification: email automatica per aggiornamenti/report

🧾 PDF Export
Servizio che genera file PDF dinamici (report, mappe, ecc.)

Endpoint API restituisce direttamente i byte[]

Frontend salva o apre il file con JSRuntime.InvokeVoidAsync("saveAs", ...)

🖼️ Appwrite – Gestione Bucket Immagini
Upload immagine effettuato via servizio backend (Appwrite SDK o REST)

URL restituito e salvato in DB

Frontend visualizza immagine pubblica o tramite URL firmato

🧱 Database & Entity Framework
Database relazionale (SQL Server / PostgreSQL)

EF Core per accesso dati e migrazioni (dotnet ef)

Repository pattern per isolamento logica accesso dati

Comandi:

bash
Copy
Edit
dotnet ef migrations add NomeMigrazione
dotnet ef database update

📦 Avvio del Progetto
bash
Copy
Edit
# Backend
cd backend
dotnet restore
dotnet run

# Frontend (Blazor WASM)
cd frontend
dotnet restore
dotnet run

# oppure build PWA
dotnet publish -c Release
