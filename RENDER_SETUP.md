# 🟣 Render — Complete Manual Setup Guide

Step-by-step configuration for **both services** through the Render dashboard.
Every field, every environment variable, every rewrite rule is listed exactly.

> **URL pattern:** Render names your services → `https://<service-name>.onrender.com`
> So if you name them exactly as shown here:
> - Backend → `https://mediconnect-api.onrender.com`
> - Frontend → `https://mediconnect-web.onrender.com`

---

## 📌 Before you start

Make sure these two things are done first:

### ✅ Check 1 — EF Core migration is committed
Open your GitHub repo and confirm the folder exists:
```
backend/MediConnect.Infrastructure/Persistence/Migrations/
```
If it does NOT exist, run this locally first:
```powershell
cd backend
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate `
  --project MediConnect.Infrastructure `
  --startup-project MediConnect.WebApi `
  --output-dir Persistence/Migrations
cd ..
git add backend/MediConnect.Infrastructure/Persistence/Migrations
git commit -m "chore: add InitialCreate EF Core migration"
git push origin main
```

### ✅ Check 2 — Have your Neon connection string ready
From **neon.tech** → your project → **Connection Details**:
- Enable **Connection Pooling: ON**
- Select format: **Parameters only** (not URL)
- Your string will look like this (copy it now):
```
Host=ep-something-pooler.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=YOURPASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

---
---

# 🔵 SERVICE 1 — Backend API (Docker Web Service)

## Step 1 — Create new Web Service

1. Log in to **[render.com](https://render.com)**
2. Click the **`New +`** button (top right)
3. Select **`Web Service`**

---

## Step 2 — Connect your GitHub repository

1. Click **`Connect account`** if GitHub is not linked yet → authorize Render
2. Search for **`MediConnectSystem`** in the repo list
3. Click **`Connect`** next to it

---

## Step 3 — Fill in the Basic Settings

Fill in these fields **exactly** as shown:

| Field | Value to enter |
|-------|---------------|
| **Name** | `mediconnect-api` |
| **Region** | `Oregon (US West)` |
| **Branch** | `main` |
| **Runtime** | `Docker` ← select this |
| **Dockerfile Path** | `./backend/Dockerfile` |
| **Docker Build Context Directory** | `.` ← just a single dot (repo root) |
| **Instance Type** | `Free` |

> ⚠️ **Dockerfile Path** and **Docker Build Context Directory** appear after
> you select `Docker` as the runtime. Make sure both are filled correctly —
> the dot (`.`) for build context is critical so the Dockerfile can see
> the `backend/` folder.

---

## Step 4 — Advanced Settings

Scroll down to **Advanced** and set:

| Field | Value |
|-------|-------|
| **Health Check Path** | `/health` |
| **Auto-Deploy** | `Yes` (toggle ON) |

Leave all other advanced fields at their defaults.

---

## Step 5 — Environment Variables

Scroll to the **Environment Variables** section.
Click **`Add Environment Variable`** for each row below.

> 📋 Copy-paste each **Key** and **Value** exactly.

### 🔑 All environment variables for the backend:

| # | Key | Value | Notes |
|---|-----|-------|-------|
| 1 | `ASPNETCORE_ENVIRONMENT` | `Production` | |
| 2 | `ConnectionStrings__Default` | *(your Neon string)* | ⚠️ Paste the full string from Check 2 above. Keep this secret. |
| 3 | `Jwt__Secret` | *(generate below)* | ⚠️ Generate a random value — see note below |
| 4 | `Jwt__Issuer` | `MediConnect` | |
| 5 | `Jwt__Audience` | `MediConnectClients` | |
| 6 | `Jwt__AccessTokenMinutes` | `60` | |
| 7 | `Jwt__RefreshTokenDays` | `7` | |
| 8 | `Cors__Origins__0` | `https://mediconnect-web.onrender.com` | Must match the frontend service name |

> **How to generate `Jwt__Secret`:**
> On Windows PowerShell run this to get a secure 64-character key:
> ```powershell
> -join ((1..64) | ForEach-Object { [char](Get-Random -Min 33 -Max 126) })
> ```
> Or use this website: **[generate-random.org/string-generator](https://generate-random.org/string-generator)**
> (Length: 64, include letters + numbers + symbols).
> Example of what it should look like:
> `x9K#mP2$vL8nQ5@jR3wY7!eA4bN6cT1uH0fG`... (64+ chars)
>
> ⚠️ Write it down somewhere safe — if you change it, all logged-in users
> will need to log in again.

---

## Step 6 — Create the Service

1. Review all settings
2. Click the **`Create Web Service`** button at the bottom of the page
3. Render starts building the Docker image — this takes **5–10 minutes** on first deploy

**Watch the Logs tab.** A successful startup looks like this:
```
==> Starting service with 'dotnet MediConnect.WebApi.dll'
info: Database migration applied.
info: Seeding complete — demo clinic ready.
info: Now listening on: http://0.0.0.0:10000
```

---

## Step 7 — Verify the backend

Once the deploy shows **`Live`**, test these URLs:

| URL | Expected response |
|-----|-------------------|
| `https://mediconnect-api.onrender.com/health` | `{"status":"healthy","timeUtc":"..."}` |
| `https://mediconnect-api.onrender.com/swagger` | Interactive API docs page |
| `https://mediconnect-api.onrender.com/` | `{"service":"MediConnect API","status":"running"}` |

✅ Backend is live. Copy the URL: **`https://mediconnect-api.onrender.com`**

---
---

# 🟢 SERVICE 2 — Frontend (Angular Static Site)

## Step 1 — Create new Static Site

1. Click **`New +`** again (top right in Render dashboard)
2. Select **`Static Site`**

---

## Step 2 — Connect the same repository

1. Search for **`MediConnectSystem`** again
2. Click **`Connect`**

---

## Step 3 — Fill in the Basic Settings

| Field | Value to enter |
|-------|---------------|
| **Name** | `mediconnect-web` |
| **Branch** | `main` |
| **Build Command** | `cd frontend && npm install && npm run build` |
| **Publish Directory** | `frontend/dist/mediconnect-web/browser` |
| **Auto-Deploy** | `Yes` (toggle ON) |

> ⚠️ The **Publish Directory** path must be exactly
> `frontend/dist/mediconnect-web/browser` — all lowercase, forward slashes.
> This matches the `outputPath` in `frontend/angular.json`.

---

## Step 4 — Redirect / Rewrite Rules

Scroll down to the **Redirects / Rewrites** section.
Click **`Add Rule`** and add these **two rules in order** (order matters):

### Rule 1 — API Proxy (must be first)

| Field | Value |
|-------|-------|
| **Source** | `/api/*` |
| **Destination** | `https://mediconnect-api.onrender.com/api/*` |
| **Action** | `Rewrite` |

> This proxies every `/api/` call to your backend **server-side**.
> The browser sees it as a same-origin request, so **no CORS issues**.
> The `Authorization: Bearer ...` header is forwarded automatically.

### Rule 2 — SPA Fallback (must be second)

| Field | Value |
|-------|-------|
| **Source** | `/*` |
| **Destination** | `/index.html` |
| **Action** | `Rewrite` |

> This lets Angular's client-side router handle deep links like
> `/app/dashboard` or `/app/appointments` without a 404 on page refresh.

---

## Step 5 — No environment variables needed

The frontend is a **static build** — the Angular app is compiled to plain
HTML/JS/CSS. The API URL is already baked in as `/api` in
`src/environments/environment.prod.ts`, which the proxy rewrite (Rule 1) handles.

You do **not** need to add any environment variables for the frontend service.

---

## Step 6 — Create the Static Site

Click **`Create Static Site`**.

Build takes **2–4 minutes**. The log will show:
```
==> Running build command: cd frontend && npm install && npm run build
...
✔ Build: Succeeded
==> Publish directory: frontend/dist/mediconnect-web/browser
==> Uploaded 47 files
==> Static site is live
```

---

## Step 7 — Verify the frontend

| URL | Expected result |
|-----|----------------|
| `https://mediconnect-web.onrender.com` | MediConnect login page loads |
| `https://mediconnect-web.onrender.com/login` | Same login page (SPA fallback works) |
| `https://mediconnect-web.onrender.com/api/health` | `{"status":"healthy",...}` (API proxy works) |

✅ Frontend is live.

---
---

# 🧪 End-to-End Login Test

Open **`https://mediconnect-web.onrender.com`** and log in:

| Account | Email | Password |
|---------|-------|----------|
| Clinic Admin | `admin@democlinic.io` | `Admin@123` |
| Super Admin | `superadmin@mediconnect.io` | `SuperAdmin@123` |

If the Dashboard loads and shows stats → everything is working correctly. 🎉

---
---

# 📊 Summary — All Render service settings at a glance

## Backend (`mediconnect-api`)

```
Type:                    Web Service
Runtime:                 Docker
Repository:              AKSHAY-PX-BYTES/MediConnectSystem
Branch:                  main
Dockerfile Path:         ./backend/Dockerfile
Docker Build Context:    .
Region:                  Oregon (US West)
Instance Type:           Free
Health Check Path:       /health
Auto-Deploy:             Yes

Environment Variables:
  ASPNETCORE_ENVIRONMENT     = Production
  ConnectionStrings__Default = Host=...neon.tech;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
  Jwt__Secret                = <your 64-char random secret>
  Jwt__Issuer                = MediConnect
  Jwt__Audience              = MediConnectClients
  Jwt__AccessTokenMinutes    = 60
  Jwt__RefreshTokenDays      = 7
  Cors__Origins__0           = https://mediconnect-web.onrender.com
```

## Frontend (`mediconnect-web`)

```
Type:              Static Site
Repository:        AKSHAY-PX-BYTES/MediConnectSystem
Branch:            main
Build Command:     cd frontend && npm install && npm run build
Publish Directory: frontend/dist/mediconnect-web/browser
Auto-Deploy:       Yes

Redirects/Rewrites:
  /api/*   → https://mediconnect-api.onrender.com/api/*   [Rewrite]
  /*       → /index.html                                   [Rewrite]

Environment Variables: (none needed)
```

---
---

# 🆘 Troubleshooting

## Backend won't start / DB error in logs

**Log says:** `relation "Users" does not exist` or `connection refused`

**Fix:**
1. Check `ConnectionStrings__Default` — paste the Neon string **exactly**, including
   `SSL Mode=Require;Trust Server Certificate=true` at the end.
2. Make sure the migration files exist in the GitHub repo:
   `backend/MediConnect.Infrastructure/Persistence/Migrations/`

---

## Frontend loads but API calls fail with network error

**Browser console shows:** `Failed to fetch` or `ERR_NAME_NOT_RESOLVED`

**Fix:** Go to `mediconnect-web` → **Redirects/Rewrites** → confirm Rule 1 exists:
- Source: `/api/*`
- Destination: `https://mediconnect-api.onrender.com/api/*`
- Action: `Rewrite`

---

## Login returns 403 Forbidden

**Fix:** The seeded accounts exist only if the migration ran successfully.
Check the API logs for `Seeding complete`. If not, verify the DB connection string.

---

## Page refresh gives 404

**Fix:** The SPA fallback rule is missing or wrong.
Go to `mediconnect-web` → **Redirects/Rewrites** → confirm Rule 2 exists:
- Source: `/*`
- Destination: `/index.html`
- Action: `Rewrite`

---

## First login takes 30–60 seconds

**This is normal on the free tier.** Render spins down free services after
15 minutes of inactivity. The first request wakes it up (cold start).
The static frontend always responds instantly — only the API cold-starts.

---

## "Service name already taken" error during Blueprint

**Fix:** Change the name in `render.yaml` to something unique
(e.g. `mediconnect-api-v2`) and update the two cross-references:
- `Cors__Origins__0` on the backend
- The `/api/*` rewrite destination on the frontend

---
---

# 🔄 Updating after initial deploy

Every `git push origin main` auto-redeploys both services.

### Adding a new database migration:
```powershell
cd backend
dotnet ef migrations add YourMigrationName `
  --project MediConnect.Infrastructure `
  --startup-project MediConnect.WebApi `
  --output-dir Persistence/Migrations
cd ..
git add backend/MediConnect.Infrastructure/Persistence/Migrations
git commit -m "feat: add YourMigrationName migration"
git push origin main
# ↑ The API automatically applies the new migration on startup
```
