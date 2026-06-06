# 🏥 MediConnect — Multi-Tenant Clinic & Appointment Management SaaS

A modern, enterprise-grade, **multi-tenant** Clinic Management & Doctor Appointment
platform built with **ASP.NET Core 9 (Clean Architecture + CQRS)** and **Angular 20**.
Designed to be sold to many clinics with **complete data isolation** between tenants.

> This repository is a runnable **foundation**: authentication, multi-tenancy,
> appointment booking, doctors & patients, billing/EMR/subscription domain models,
> Swagger, Docker, and a PWA-ready Angular SPA. It is structured so the remaining
> modules (prescriptions UI, billing UI, reporting, notifications) slot in cleanly.

---

## 🧱 Architecture

```
MediConnectSystem/
├─ backend/
│  ├─ MediConnect.Domain          # Entities, enums, base types (no dependencies)
│  ├─ MediConnect.Application      # CQRS (MediatR), DTOs, validation, interfaces
│  ├─ MediConnect.Infrastructure   # EF Core (Npgsql), tenant filters, JWT, seeding
│  └─ MediConnect.WebApi           # Controllers, middleware, DI, Swagger
├─ frontend/                       # Angular 20 standalone + Material + Tailwind + PWA
├─ docker-compose.yml              # db + api + web
└─ MediConnect.sln
```

**Patterns:** Clean Architecture · CQRS · Repository-style `IApplicationDbContext`
· Pipeline validation behaviour · Result/Problem-Details error handling.

### 🔐 Multi-Tenant Isolation
Every clinic is a `Tenant`. Tenant-scoped entities implement `ITenantScoped` and are
filtered automatically by an **EF Core global query filter** that references the
DbContext instance fields (`_currentTenantId`, `_isSuperAdmin`) so the cached model
stays correct per request. The tenant id is carried in the JWT (`tenant_id` claim)
and resolved by `CurrentUserService`. **Super Admins** bypass tenant scoping.

---

## 🚀 Getting Started

### Prerequisites
- .NET SDK 9
- Node.js 20+ and npm
- A PostgreSQL database (local Docker, or a free **Neon** instance)

### 1) Configure the database
Edit `backend/MediConnect.WebApi/appsettings.json` → `ConnectionStrings:Default`
with your Neon/Postgres connection string, and set a strong `Jwt:Secret` (≥ 32 chars).

You can also override via environment variables:
```
ConnectionStrings__Default="Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
Jwt__Secret="your-very-long-random-secret"
```

### 2) Create the initial EF Core migration
From the `backend` folder (run once):
```powershell
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add InitialCreate `
  --project MediConnect.Infrastructure `
  --startup-project MediConnect.WebApi `
  --output-dir Persistence/Migrations
```
> The API applies migrations **and seeds** baseline data automatically on startup.

### 3) Run the backend
```powershell
cd backend
dotnet run --project MediConnect.WebApi
```
Swagger UI → http://localhost:5080/swagger

### 4) Run the frontend
```powershell
cd frontend
npm install
npm start
```
App → http://localhost:4200

---

## 🔑 Seeded Accounts

| Role         | Email                          | Password         |
|--------------|--------------------------------|------------------|
| Super Admin  | superadmin@mediconnect.io      | `SuperAdmin@123` |
| Clinic Admin | admin@democlinic.io            | `Admin@123`      |

The demo clinic comes with sample departments, doctors, and a patient.

---

## 🐳 Run everything with Docker

```powershell
docker compose up --build
```
- Web (Nginx + Angular): http://localhost:8080
- API (Swagger): http://localhost:5080/swagger
- PostgreSQL: localhost:5432

---

## 🧩 Implemented Modules

| Area | Status |
|------|--------|
| Multi-tenant data isolation (EF global filters) | ✅ |
| JWT auth + refresh-token rotation + RBAC | ✅ |
| Self-service clinic onboarding + trial subscription | ✅ |
| Doctors / Departments / Patients | ✅ (API) |
| Appointment booking + status workflow + queue numbers | ✅ |
| Domain models: EMR, Prescriptions, Billing, Subscriptions, Audit | ✅ |
| Angular SPA: login, register, dashboard, appointments, dark mode, PWA | ✅ |
| Prescription / Billing / Reporting UIs, Notifications | 🔜 (scaffolded domain ready) |

## 🗺️ Roadmap (per spec)
Queue live updates (SignalR) · PDF/Excel reporting · Email/WhatsApp notifications ·
Cloudinary uploads · Telemedicine/video · AI assistant.

---

## 🔒 Security Notes
- Passwords hashed with **BCrypt** (work factor 12).
- JWT access tokens (60 min) + rotating refresh tokens (7 days).
- Soft-delete + audit fields on all entities; immutable `AuditLog`.
- Tenant isolation enforced at the data layer, not just the UI.
- Configure secrets via environment variables / user-secrets in production —
  never commit real connection strings or JWT secrets.

---

## 📄 API quick test
Open `backend/MediConnect.WebApi/MediConnect.http` in VS Code (REST Client) to
register a clinic, log in, and exercise the appointment endpoints.
