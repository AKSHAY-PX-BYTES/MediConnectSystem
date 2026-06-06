# 🚀 MediConnect — Production Deployment Guide

Both the **backend API** and the **frontend** are deployed on **Render**, driven by
the single `render.yaml` Blueprint already committed to the repo.

```
Browser
  │
  ▼
Render Static Site  (Angular PWA — mediconnect-web.onrender.com)
  │  /api/* proxy (server-side, same-origin to the browser)
  ▼
Render Docker Service  (ASP.NET Core 9 — mediconnect-api.onrender.com)
  │
  ▼
Neon PostgreSQL  (free, serverless, always-on)
```

> **Why proxy?**  The static site rewrites `/api/*` to the API service
> **on the server**, so the browser makes same-origin requests. No CORS
> config needed, no tokens exposed in extra network hops.

---

## 🧩 What the Blueprint (`render.yaml`) creates for you

| Service | Type | URL |
|---------|------|-----|
| `mediconnect-api` | Docker Web Service | `https://mediconnect-api.onrender.com` |
| `mediconnect-web` | Static Site | `https://mediconnect-web.onrender.com` |

All env vars are pre-wired in `render.yaml`. You only need to paste **one secret**
(the Neon database connection string) after the Blueprint is applied.

---

## ✅ Prerequisites

- Code on GitHub at `https://github.com/AKSHAY-PX-BYTES/MediConnectSystem`
- Free accounts at **[neon.tech](https://neon.tech)** and **[render.com](https://render.com)**
- **.NET SDK 9** installed locally (needed once, for the migration step only)

---

## 📋 STEP 0 — Generate the EF Core migration (local, one time only)

> ⚠️ **Required before first deploy.** The API runs `dotnet ef database update`
> automatically on startup. Without migration files committed, the database tables
> are never created and the startup seed fails.

```powershell
cd backend

# Install the EF tools globally (skip if already installed)
dotnet tool install --global dotnet-ef

# Create the migration
dotnet ef migrations add InitialCreate `
  --project MediConnect.Infrastructure `
  --startup-project MediConnect.WebApi `
  --output-dir Persistence/Migrations

# Commit the generated files
cd ..
git add backend/MediConnect.Infrastructure/Persistence/Migrations
git commit -m "chore: add InitialCreate EF Core migration"
git push origin main
```

✔️ Confirm a folder `backend/MediConnect.Infrastructure/Persistence/Migrations/`
exists and contains `*_InitialCreate.cs` before continuing.

---

## 🗄️ STEP 1 — Create the Neon PostgreSQL database

1. Log in at **[neon.tech](https://neon.tech)** → click **New Project**.
2. Fill in:
   - **Name:** `mediconnect`
   - **Region:** `AWS US East (Ohio)` ← same region as Render Oregon reduces latency
   - Leave defaults → **Create Project**.
3. On the project dashboard go to **Connection Details**.
   - Toggle **Connection Pooling: ON** (important for serverless).
   - From the dropdown pick **Connection string → .NET (Npgsql)** format.
4. **Copy the connection string.** It looks like:
   ```
   Host=ep-cool-name-pooler.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=XXXXX;SSL Mode=Require;Trust Server Certificate=true
   ```
   > Keep this string open in a browser tab — you'll paste it in **Step 3**.

---

## 🚀 STEP 2 — Deploy BOTH services on Render (Blueprint, one click)

1. Log in at **[render.com](https://render.com)**.
2. Click **New +** → **Blueprint**.
3. Connect GitHub if prompted → select the repo **`MediConnectSystem`**.
4. Render reads `render.yaml` and shows a preview of **two services**:
   - `mediconnect-api` (Docker web service)
   - `mediconnect-web` (Static site)
5. Click **Apply Blueprint**.

Render now queues both services. The **frontend build** finishes first (2–3 min).
The **backend build** takes longer (5–8 min, Docker image).

> ⏳ Do **not** close this tab — proceed to Step 3 while the build runs.

---

## 🔑 STEP 3 — Paste the Neon secret (the only manual step)

The Blueprint pre-fills every env var **except** the database connection string
(intentionally kept out of git).

1. In the Render dashboard go to **`mediconnect-api`** → **Environment**.
2. Find `ConnectionStrings__Default` (marked *"Value required"*).
3. Click **Edit** → paste the Neon connection string from Step 1.
4. Click **Save Changes**.

Render automatically redeploys the API. Watch the **Logs** tab — look for:

```
info: Database migration applied successfully.
info: Seeding complete.
info: Now listening on: http://0.0.0.0:XXXX
```

---

## ✅ STEP 4 — Verify both services are live

### Backend health check
Open: `https://mediconnect-api.onrender.com/health`
```json
{ "status": "healthy", "timeUtc": "2026-06-06T..." }
```

### Swagger (interactive API docs)
Open: `https://mediconnect-api.onrender.com/swagger`
You should see the full MediConnect API with a JWT auth button.

### Frontend
Open: `https://mediconnect-web.onrender.com`
You should see the MediConnect login screen.

### End-to-end test
Log in using the seeded accounts:

| Role | Email | Password |
|------|-------|----------|
| Clinic Admin | `admin@democlinic.io` | `Admin@123` |
| Super Admin | `superadmin@mediconnect.io` | `SuperAdmin@123` |

Confirm the **Dashboard** stats load and the **Appointments** list appears.
Register a new clinic from the *Create account* page to test the full onboarding flow.

---

## 🔄 Continuous deployment (auto after initial setup)

Everything auto-deploys on every `git push origin main`:
- Render rebuilds **both** services from the updated code.
- New EF migrations? Create them locally, commit, push — the API applies them on the next startup.

```powershell
# Example: add a new feature migration
dotnet ef migrations add AddNotificationsTable `
  --project backend/MediConnect.Infrastructure `
  --startup-project backend/MediConnect.WebApi `
  --output-dir Persistence/Migrations
git add backend/MediConnect.Infrastructure/Persistence/Migrations
git commit -m "feat: add notifications migration"
git push origin main
# ↑ Render auto-builds and the API auto-migrates on startup
```

---

## 🌍 (Optional) Add a custom domain

**Frontend (`mediconnect-web`):**
1. Render Dashboard → `mediconnect-web` → **Settings → Custom Domains** → Add domain.
2. Add the CNAME record shown to your DNS provider.
3. Render provisions the SSL certificate automatically (~2 min).
4. Update `Cors__Origins__0` on the API service to the new domain.

**Backend (`mediconnect-api`):**
1. Same process in the API service settings.
2. Update the `/api/*` rewrite destination in `render.yaml` and push.

---

## 🆘 Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| API logs: *"relation does not exist"* | Migration files not committed | Do **Step 0**, push the `Migrations/` folder |
| API logs: DB timeout / SSL error | Wrong connection string | Use the **pooled** Neon host with `SSL Mode=Require;Trust Server Certificate=true` |
| Frontend loads but clicking anything shows 404 | SPA fallback missing | `render.yaml` rewrite `/* → /index.html` is present — redeploy frontend |
| API calls return `404` from frontend | Proxy rewrite not applied | Ensure `render.yaml` has `/api/*` → `https://mediconnect-api.onrender.com/api/*` and redeploy |
| `401 Unauthorized` on every call | JWT secret mismatch after redeploy | `Jwt__Secret` uses `generateValue: true` — only changes on manual reset. Re-login clears it |
| First request is slow (~30–60 s) | Free tier cold start after 15 min idle | Expected on free plan. The static site always responds instantly; only the API cold-starts |
| Blueprint shows "service name taken" | Another Render account owns the name | Rename services in `render.yaml` and update the cross-references (`Cors__Origins__0` and the `/api/*` rewrite destination) |

---

## 🐳 Alternative: local Docker (all-in-one)

The `docker-compose.yml` runs db + api + web together (nginx proxies `/api` to the
API container internally):

```powershell
# From the repo root:
docker compose up --build -d
# Web  → http://localhost:8080
# API  → http://localhost:5080/swagger
```

---

## 🔒 Production security checklist

- [ ] `Jwt__Secret` is set via `generateValue: true` — never the placeholder
- [ ] Neon connection string is in the Render secret store only, never in git
- [ ] Change the seeded `Admin@123` / `SuperAdmin@123` passwords before real patient use
- [ ] Enable Neon automatic backups (free tier supports daily backups)
- [ ] Add a custom domain with HTTPS once you go live with real users
|-------|---------|-----|
| **Database** | [Neon](https://neon.tech) PostgreSQL | Free, always-on, serverless Postgres |
| **Backend API** | [Render](https://render.com) (Docker web service) | Free Docker hosting, auto-deploy from GitHub |
| **Frontend** | [Netlify](https://netlify.com) *or* [Vercel](https://vercel.com) | Free static hosting + API proxy |

> Architecture in production:
> `Browser → Netlify/Vercel (Angular static + /api proxy) → Render (ASP.NET Core API) → Neon (PostgreSQL)`

The frontend proxies `/api/*` to the backend **server-side**, so the browser sees
same-origin requests and **no CORS setup is needed** on the happy path.

---

## ✅ Prerequisites

- Code pushed to GitHub: `https://github.com/AKSHAY-PX-BYTES/MediConnectSystem`
- Free accounts: **Neon**, **Render**, and **Netlify** (or **Vercel**)
- Locally installed: **.NET SDK 9** and **Git** (only needed once, for the migration step)

---

## 🧩 STEP 0 — One-time prep: generate the database migration (REQUIRED)

The API applies EF Core migrations automatically on startup. The repo ships
**without** a generated migration, so you must create and commit it once — otherwise
the database tables are never created and startup seeding fails.

```powershell
cd backend
dotnet tool install --global dotnet-ef      # skip if already installed

dotnet ef migrations add InitialCreate `
  --project MediConnect.Infrastructure `
  --startup-project MediConnect.WebApi `
  --output-dir Persistence/Migrations
```

Commit and push the generated files:

```powershell
cd ..
git add backend/MediConnect.Infrastructure/Persistence/Migrations
git commit -m "Add InitialCreate EF Core migration"
git push origin main
```

> ✔️ Verify a folder `backend/MediConnect.Infrastructure/Persistence/Migrations/`
> now exists with `*_InitialCreate.cs` files.

---

## 🗄️ STEP 1 — Create the Neon PostgreSQL database

1. Sign in at **https://neon.tech** → **New Project**.
2. Name it `mediconnect`, pick the region closest to your Render region (e.g. *US East (Ohio)* ↔ Render *Oregon/Ohio*), click **Create**.
3. Open **Dashboard → Connection Details**. Toggle **Connection pooling = ON** and copy the **pooled** host.
4. Note these five values:
   - **Host** (the `-pooler` one, e.g. `ep-cool-name-pooler.us-east-2.aws.neon.tech`)
   - **Database** (e.g. `neondb` or `mediconnect`)
   - **User**
   - **Password**
   - **Port** = `5432`

5. Build the **.NET / Npgsql** connection string (note: Npgsql uses `key=value;`,
   **not** the `postgresql://...` URL Neon shows by default):

```
Host=ep-cool-name-pooler.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

Keep this string handy — you'll paste it into Render in the next step.

---

## 🔧 STEP 2 — Deploy the backend API to Render

You can use the included **Blueprint** (`render.yaml`) or configure manually.

### Option A — Blueprint (recommended, one click)

1. Go to **https://dashboard.render.com** → **New +** → **Blueprint**.
2. Connect your GitHub and select **MediConnectSystem**. Render detects `render.yaml`.
3. Click **Apply**. It creates a service named **mediconnect-api**.
4. Open the service → **Environment** and set the two secrets marked "set in dashboard":
   - `ConnectionStrings__Default` → the Neon string from Step 1.
   - `Cors__Origins__0` → leave a placeholder for now (e.g. `https://localhost`); you'll update it in Step 4 once you have the frontend URL.
   - `Jwt__Secret` is auto-generated for you. ✔️
5. **Save Changes** → Render builds the Docker image and deploys.

### Option B — Manual setup

1. **New +** → **Web Service** → connect the repo.
2. Configure:
   - **Runtime:** `Docker`
   - **Dockerfile Path:** `backend/Dockerfile`
   - **Docker Build Context Directory:** `.` (repo root)
   - **Plan:** Free
   - **Health Check Path:** `/health`
3. Add **Environment Variables** (from `.env.example`):

   | Key | Value |
   |-----|-------|
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `ConnectionStrings__Default` | *(Neon string from Step 1)* |
   | `Jwt__Secret` | *(a 48-char random string)* |
   | `Jwt__Issuer` | `MediConnect` |
   | `Jwt__Audience` | `MediConnectClients` |
   | `Cors__Origins__0` | *(frontend URL — set in Step 4)* |

4. **Create Web Service**. First build takes a few minutes.

### Verify the backend

When the log shows it's live, open:
- `https://mediconnect-api.onrender.com/health` → `{"status":"healthy",...}`
- `https://mediconnect-api.onrender.com/swagger` → interactive API docs

> 📝 Copy your real backend URL (e.g. `https://mediconnect-api.onrender.com`).
> Free services **cold-start** after ~15 min idle — the first request may take 30–60s.

---

## 🌐 STEP 3 — Deploy the frontend

Pick **one** host. Both proxy `/api` to your backend, so the Angular app keeps
using `apiUrl: '/api'` (already set in `environment.prod.ts`).

### Option A — Netlify

1. Edit **`netlify.toml`** (repo root) and replace `YOUR-BACKEND.onrender.com`
   with your real Render host. Commit & push:
   ```powershell
   git add netlify.toml
   git commit -m "Point Netlify API proxy at Render backend"
   git push origin main
   ```
2. At **https://app.netlify.com** → **Add new site → Import an existing project** → pick the repo.
3. Netlify reads `netlify.toml` automatically (build command + publish dir are preset). Click **Deploy**.
4. After deploy you'll get a URL like `https://mediconnect.netlify.app`.

### Option B — Vercel

1. Edit **`frontend/vercel.json`** and replace `YOUR-BACKEND.onrender.com`. Commit & push.
2. At **https://vercel.com/new** → import the repo.
3. Set **Root Directory** to `frontend`. Vercel reads `vercel.json` for build + rewrites.
4. **Deploy** → you'll get `https://mediconnect.vercel.app`.

---

## 🔗 STEP 4 — Connect frontend ↔ backend (CORS)

Because the host proxy makes requests same-origin, CORS usually isn't required.
But set it anyway for safety (and it's needed if you ever call the API directly):

1. In **Render → mediconnect-api → Environment**, set:
   ```
   Cors__Origins__0 = https://mediconnect.netlify.app   (your real frontend URL)
   ```
2. **Save** (the service restarts automatically).

---

## 🧪 STEP 5 — Verify the live app

1. Open your frontend URL.
2. Log in with a seeded account:
   - **Clinic Admin:** `admin@democlinic.io` / `Admin@123`
   - **Super Admin:** `superadmin@mediconnect.io` / `SuperAdmin@123`
3. Confirm the **Dashboard** loads stats and **Appointments** lists data.
4. Register a brand-new clinic from the **Create account** page to confirm tenant onboarding.

✅ You're live!

---

## 🔄 Updating the app later

Everything is wired for **continuous deployment**:
- `git push origin main` → Render rebuilds the API, Netlify/Vercel rebuilds the frontend.
- New EF migrations: create them locally (`dotnet ef migrations add <Name> ...`), commit, push — they apply automatically on the next backend startup.

---

## 🌍 (Optional) Custom domain

- **Frontend:** Netlify/Vercel → *Domain settings* → add your domain, follow DNS steps (free HTTPS included).
- **Backend:** Render → *Settings → Custom Domains*. Then update the frontend proxy target and `Cors__Origins__0` to match.

---

## 🆘 Troubleshooting

| Symptom | Likely cause | Fix |
|--------|--------------|-----|
| Backend logs: *"relation does not exist"* / seeding error | Migration not generated | Do **Step 0**, commit the `Migrations` folder, redeploy |
| Backend won't start, DB timeout | Wrong/incomplete connection string | Use the **pooled** Neon host + `SSL Mode=Require;Trust Server Certificate=true` |
| `401` on every API call | `Jwt__Secret` differs from when token issued / too short | Use one stable secret ≥ 32 chars; re-login |
| Frontend loads but API calls 404 | Proxy target not updated | Replace `YOUR-BACKEND.onrender.com` in `netlify.toml` / `vercel.json` |
| CORS error in console | Calling API cross-origin without proxy | Set `Cors__Origins__0` to the exact frontend origin (no trailing slash) |
| First request very slow | Render free cold start | Expected after idle; upgrade plan or ping `/health` periodically |
| Refreshing a deep link shows 404 | Missing SPA fallback | Ensure the `/* → /index.html` rule exists (it's in the provided configs) |

---

## 🐳 Alternative — all-in-one Docker (VPS / Railway / Fly.io)

The repo's `docker-compose.yml` runs **db + api + web** together (frontend's
`nginx.conf` already proxies `/api` to the API container):

```bash
# On any Docker host:
git clone https://github.com/AKSHAY-PX-BYTES/MediConnectSystem
cd MediConnectSystem
# edit docker-compose.yml: set a strong Jwt__Secret (and a real DB if not using the bundled one)
docker compose up --build -d
```
- Web → `http://<host>:8080`
- API → `http://<host>:5080/swagger`

For **Railway**: create a project from the repo, add a PostgreSQL plugin, and deploy
the `backend/Dockerfile` and `frontend/Dockerfile` as two services (set the same
env vars as Step 2). For **Fly.io**: `fly launch` in `backend/`, set secrets with
`fly secrets set Jwt__Secret=... ConnectionStrings__Default=...`.

---

## 🔒 Production hardening checklist

- [ ] `Jwt__Secret` is a unique, random value (never the placeholder).
- [ ] Real connection string lives only in the host's secret store, never in git.
- [ ] Change/disable the seeded demo accounts before real use.
- [ ] Consider hiding Swagger in production (guard `app.UseSwagger()` behind `IsDevelopment()` in `Program.cs`).
- [ ] Set up automated Neon backups / branching.
- [ ] Add a custom domain with HTTPS (automatic on all three providers).
- [ ] Configure Cloudinary keys when you enable file uploads.
