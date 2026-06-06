import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  styles: [`
    :host { display: block; height: 100vh; }
    .left-panel {
      background: linear-gradient(145deg, #0c1445 0%, #0d2257 40%, #0f3375 70%, #1a4b9c 100%);
    }
    .grid-bg {
      background-image:
        linear-gradient(rgba(255,255,255,.04) 1px, transparent 1px),
        linear-gradient(90deg, rgba(255,255,255,.04) 1px, transparent 1px);
      background-size: 40px 40px;
    }
    .feature-item { transition: transform .2s; }
    .feature-item:hover { transform: translateX(4px); }
  `],
  template: `
  <div class="flex h-screen overflow-hidden">

    <!-- ═══ LEFT — Branding Panel ═══════════════════════════════════════ -->
    <div class="left-panel hidden lg:flex lg:w-[56%] flex-col justify-between p-14 relative overflow-hidden">

      <!-- Background grid + glows -->
      <div class="grid-bg absolute inset-0 pointer-events-none"></div>
      <div class="absolute top-0 right-0 w-80 h-80 bg-blue-500 opacity-10 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute bottom-0 left-0 w-96 h-96 bg-indigo-600 opacity-10 rounded-full blur-3xl pointer-events-none"></div>

      <!-- Logo -->
      <div class="relative z-10 flex items-center gap-3">
        <div class="w-10 h-10 bg-blue-500 rounded-xl flex items-center justify-center shadow-lg shadow-blue-500/30">
          <span class="material-symbols-outlined text-white" style="font-size:1.25rem">health_and_safety</span>
        </div>
        <span class="text-white text-xl font-bold tracking-tight">MediConnect</span>
      </div>

      <!-- Hero content -->
      <div class="relative z-10">
        <div class="inline-flex items-center gap-2 bg-blue-500/10 border border-blue-400/20 rounded-full px-3.5 py-1.5 mb-6">
          <span class="w-2 h-2 bg-green-400 rounded-full animate-pulse"></span>
          <span class="text-blue-200 text-xs font-medium">Trusted by 500+ healthcare providers</span>
        </div>

        <h1 class="text-4xl font-bold text-white leading-snug mb-4">
          Modern clinic management<br>
          <span class="text-transparent bg-clip-text" style="background-image:linear-gradient(90deg,#60a5fa,#a78bfa)">
            built for doctors
          </span>
        </h1>
        <p class="text-blue-200 text-lg leading-relaxed mb-10 max-w-md">
          Streamline appointments, digitize records, and grow your practice — all in one secure multi-tenant platform.
        </p>

        <!-- Feature list -->
        <div class="space-y-4">
          @for (f of features; track f.title) {
            <div class="feature-item flex items-start gap-4">
              <div class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
                   style="background:rgba(255,255,255,.07); border:1px solid rgba(255,255,255,.12)">
                <span class="material-symbols-outlined text-blue-300" style="font-size:1.1rem">{{ f.icon }}</span>
              </div>
              <div>
                <p class="text-white text-sm font-semibold">{{ f.title }}</p>
                <p class="text-blue-300 text-xs mt-0.5">{{ f.desc }}</p>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Stat strip -->
      <div class="relative z-10 grid grid-cols-3 gap-3">
        @for (s of stats; track s.label) {
          <div class="rounded-xl p-4 text-center" style="background:rgba(255,255,255,.05); border:1px solid rgba(255,255,255,.08)">
            <p class="text-2xl font-bold text-white">{{ s.value }}</p>
            <p class="text-blue-300 text-xs mt-0.5">{{ s.label }}</p>
          </div>
        }
      </div>
    </div>

    <!-- ═══ RIGHT — Form Panel ════════════════════════════════════════════ -->
    <div class="w-full lg:w-[44%] flex items-center justify-center p-6 sm:p-12 bg-white overflow-y-auto">
      <div class="w-full max-w-[380px] animate-slide-up">

        <!-- Mobile logo -->
        <div class="flex items-center gap-2 mb-10 lg:hidden">
          <div class="w-8 h-8 bg-brand-600 rounded-lg flex items-center justify-center">
            <span class="material-symbols-outlined text-white" style="font-size:1rem">health_and_safety</span>
          </div>
          <span class="text-slate-800 font-bold text-lg">MediConnect</span>
        </div>

        <!-- Heading -->
        <h2 class="text-2xl font-bold text-slate-900 mb-1">Welcome back</h2>
        <p class="text-slate-500 text-sm mb-8">Sign in to your clinic workspace</p>

        <form [formGroup]="form" (ngSubmit)="submit()">

          <!-- Email -->
          <div class="mb-4">
            <label class="block text-sm font-semibold text-slate-700 mb-1.5">Email address</label>
            <input
              type="email"
              formControlName="email"
              autocomplete="username"
              placeholder="you@clinic.com"
              class="mc-input"
              [class.mc-input--error]="f['email'].invalid && f['email'].touched"
            />
            @if (f['email'].invalid && f['email'].touched) {
              <p class="mt-1 text-xs text-red-500">Enter a valid email address.</p>
            }
          </div>

          <!-- Password -->
          <div class="mb-5">
            <label class="block text-sm font-semibold text-slate-700 mb-1.5">Password</label>
            <div class="relative">
              <input
                [type]="showPwd() ? 'text' : 'password'"
                formControlName="password"
                autocomplete="current-password"
                placeholder="••••••••"
                class="mc-input"
                style="padding-right:2.75rem"
              />
              <button
                type="button"
                (click)="togglePwd()"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                style="background:transparent;border:none;padding:0;cursor:pointer"
              >
                <span class="material-symbols-outlined" style="font-size:1.2rem">
                  {{ showPwd() ? 'visibility_off' : 'visibility' }}
                </span>
              </button>
            </div>
          </div>

          <!-- Error banner -->
          @if (error()) {
            <div class="flex items-center gap-2.5 p-3 mb-4 rounded-lg"
                 style="background:#fef2f2; border:1px solid #fecaca">
              <span class="material-symbols-outlined text-red-500" style="font-size:1rem">error</span>
              <p class="text-sm text-red-700">{{ error() }}</p>
            </div>
          }

          <!-- Submit -->
          <button type="submit" class="mc-btn mc-btn--primary w-full h-12 text-base" [disabled]="loading()">
            @if (loading()) {
              <svg class="w-4 h-4 animate-spin-slow" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
              </svg>
              <span>Signing in…</span>
            } @else {
              <span>Sign in</span>
              <span class="material-symbols-outlined" style="font-size:1.1rem">arrow_forward</span>
            }
          </button>
        </form>

        <!-- Register link -->
        <p class="text-center text-sm text-slate-500 mt-6">
          New to MediConnect?&nbsp;
          <a routerLink="/register" class="text-brand-600 font-semibold hover:text-brand-700">Create clinic account</a>
        </p>

        <!-- Demo credentials box -->
        <div class="mt-8 rounded-xl p-4" style="background:#f0f7ff; border:1px solid #bfdbfe">
          <p class="text-xs font-bold text-brand-700 uppercase tracking-wider mb-3">Demo credentials</p>
          @for (d of demos; track d.label) {
            <div class="flex items-center justify-between mb-2 last:mb-0">
              <div>
                <p class="text-xs font-semibold text-slate-700">{{ d.label }}</p>
                <p class="text-xs text-slate-400">{{ d.email }}</p>
              </div>
              <button
                type="button"
                (click)="fillDemo(d.email, d.password)"
                class="mc-btn mc-btn--ghost mc-btn--sm"
              >Use</button>
            </div>
          }
        </div>

      </div>
    </div>

  </div>
  `
})
export class LoginComponent {
  private readonly fb  = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading  = signal(false);
  readonly error    = signal<string | null>(null);
  readonly showPwd  = signal(false);

  readonly form = this.fb.nonNullable.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  get f() { return this.form.controls; }

  readonly features = [
    { icon: 'event_available', title: 'Smart appointment scheduling',    desc: 'Automated slots, queue management & reminders'   },
    { icon: 'shield',          title: 'Secure multi-tenant isolation',   desc: 'Every clinic\'s data is fully independent'       },
    { icon: 'description',     title: 'Digital prescriptions & EMR',     desc: 'Complete electronic medical records in one place'},
  ];

  readonly stats = [
    { value: '500+', label: 'Clinics' },
    { value: '50K+', label: 'Patients' },
    { value: '99.9%',label: 'Uptime'   },
  ];

  readonly demos = [
    { label: 'Clinic Admin', email: 'admin@democlinic.io',       password: 'Admin@123'      },
    { label: 'Super Admin',  email: 'superadmin@mediconnect.io', password: 'SuperAdmin@123' },
  ];

  fillDemo(email: string, password: string): void {
    this.form.patchValue({ email, password });
  }

  togglePwd(): void { this.showPwd.set(!this.showPwd()); }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);
    this.auth.login(this.form.getRawValue()).subscribe({
      next:  ()    => this.router.navigate(['/app/dashboard']),
      error: (err: { error?: { title?: string } }) => {
        this.error.set(err?.error?.title ?? 'Invalid email or password.');
        this.loading.set(false);
      },
    });
  }
}
