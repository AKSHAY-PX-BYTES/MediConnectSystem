import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  styles: [`
    :host { display: block; min-height: 100vh; }
    .right-panel {
      background: linear-gradient(145deg, #0c1445 0%, #0d2257 40%, #0f3375 70%, #1a4b9c 100%);
    }
    .grid-bg {
      background-image:
        linear-gradient(rgba(255,255,255,.04) 1px, transparent 1px),
        linear-gradient(90deg, rgba(255,255,255,.04) 1px, transparent 1px);
      background-size: 40px 40px;
    }
  `],
  template: `
  <div class="flex min-h-screen">
    <!-- ═══ LEFT — Form Panel ════════════════════════════════════════════ -->
    <div class="w-full lg:w-[55%] flex items-start justify-center p-8 sm:p-12 bg-white overflow-y-auto">
      <div class="w-full max-w-[480px] animate-slide-up py-4">
        <div class="flex items-center gap-2.5 mb-10">
          <div class="w-9 h-9 bg-brand-600 rounded-xl flex items-center justify-center shadow-md">
            <span class="material-symbols-outlined text-white" style="font-size:1.1rem">health_and_safety</span>
          </div>
          <span class="text-slate-800 font-bold text-lg">MediConnect</span>
        </div>
        <h2 class="text-2xl font-bold text-slate-900 mb-1">Start your free trial</h2>
        <p class="text-slate-500 text-sm mb-8">14 days free · No credit card required · Cancel anytime</p>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="mb-4">
            <label class="block text-sm font-semibold text-slate-700 mb-1.5">Clinic name</label>
            <input type="text" formControlName="clinicName" placeholder="e.g. Bright Smile Dental" class="mc-input"
              [class.mc-input--error]="f['clinicName'].invalid && f['clinicName'].touched" />
            @if (f['clinicName'].invalid && f['clinicName'].touched) {
              <p class="mt-1 text-xs text-red-500">Clinic name is required.</p>
            }
          </div>
          <div class="grid grid-cols-2 gap-3 mb-4">
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1.5">First name</label>
              <input type="text" formControlName="adminFirstName" placeholder="Priya" class="mc-input"
                [class.mc-input--error]="f['adminFirstName'].invalid && f['adminFirstName'].touched" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1.5">Last name</label>
              <input type="text" formControlName="adminLastName" placeholder="Sharma" class="mc-input"
                [class.mc-input--error]="f['adminLastName'].invalid && f['adminLastName'].touched" />
            </div>
          </div>
          <div class="mb-4">
            <label class="block text-sm font-semibold text-slate-700 mb-1.5">Work email</label>
            <input type="email" formControlName="email" placeholder="you@clinic.com" class="mc-input"
              [class.mc-input--error]="f['email'].invalid && f['email'].touched" />
            @if (f['email'].invalid && f['email'].touched) {
              <p class="mt-1 text-xs text-red-500">Enter a valid email address.</p>
            }
          </div>
          <div class="grid grid-cols-2 gap-3 mb-5">
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1.5">
                Phone <span class="text-slate-400 font-normal">(optional)</span>
              </label>
              <input type="tel" formControlName="phoneNumber" placeholder="+91 9000000000" class="mc-input" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-slate-700 mb-1.5">Password</label>
              <div class="relative">
                <input [type]="showPwd() ? 'text' : 'password'" formControlName="password"
                  placeholder="Min 8 chars" class="mc-input" style="padding-right:2.75rem"
                  [class.mc-input--error]="f['password'].invalid && f['password'].touched" />
                <button type="button" (click)="togglePwd()"
                  style="position:absolute;right:.75rem;top:50%;transform:translateY(-50%);background:transparent;border:none;padding:0;cursor:pointer;color:#94a3b8">
                  <span class="material-symbols-outlined" style="font-size:1.1rem">
                    {{ showPwd() ? 'visibility_off' : 'visibility' }}
                  </span>
                </button>
              </div>
              @if (f['password'].invalid && f['password'].touched) {
                <p class="mt-1 text-xs text-red-500">Min 8 characters required.</p>
              }
            </div>
          </div>
          @if (error()) {
            <div class="flex items-center gap-2.5 p-3 mb-4 rounded-lg"
                 style="background:#fef2f2; border:1px solid #fecaca">
              <span class="material-symbols-outlined text-red-500" style="font-size:1rem">error</span>
              <p class="text-sm text-red-700">{{ error() }}</p>
            </div>
          }
          <button type="submit" class="mc-btn mc-btn--primary w-full h-12 text-base" [disabled]="loading()">
            @if (loading()) {
              <svg class="w-4 h-4 animate-spin-slow" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
              </svg>
              <span>Creating account…</span>
            } @else {
              <span>Create clinic account</span>
              <span class="material-symbols-outlined" style="font-size:1.1rem">arrow_forward</span>
            }
          </button>
        </form>
        <p class="text-center text-sm text-slate-500 mt-6">
          Already have an account?&nbsp;
          <a routerLink="/login" class="text-brand-600 font-semibold hover:text-brand-700">Sign in</a>
        </p>
        <p class="text-center text-xs text-slate-400 mt-4">
          By creating an account you agree to our Terms of Service and Privacy Policy.
        </p>
      </div>
    </div>
    <!-- ═══ RIGHT — Benefits Panel ════════════════════════════════════════ -->
    <div class="right-panel hidden lg:flex lg:w-[45%] flex-col justify-between p-12 relative overflow-hidden">
      <div class="grid-bg absolute inset-0 pointer-events-none"></div>
      <div class="absolute top-0 left-0 w-80 h-80 bg-blue-500 opacity-10 rounded-full blur-3xl pointer-events-none"></div>
      <div class="relative z-10">
        <span class="text-white font-bold text-lg">Trusted by healthcare professionals</span>
      </div>
      <div class="relative z-10 space-y-4">
        @for (b of benefits; track b.title) {
          <div class="flex items-start gap-4 p-4 rounded-xl"
               style="background:rgba(255,255,255,.06); border:1px solid rgba(255,255,255,.1)">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 bg-blue-500/20">
              <span class="material-symbols-outlined text-blue-300" style="font-size:1.2rem">{{ b.icon }}</span>
            </div>
            <div>
              <p class="text-white text-sm font-semibold mb-0.5">{{ b.title }}</p>
              <p class="text-blue-200 text-xs leading-relaxed">{{ b.desc }}</p>
            </div>
          </div>
        }
      </div>
      <div class="relative z-10 rounded-2xl p-6" style="background:rgba(255,255,255,.06); border:1px solid rgba(255,255,255,.1)">
        <p class="text-blue-100 text-sm leading-relaxed italic mb-4">
          "MediConnect cut our appointment booking time by 80%. Our patients love the digital reminders."
        </p>
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-full bg-blue-400/20 flex items-center justify-center">
            <span class="material-symbols-outlined text-blue-300" style="font-size:1.2rem">person</span>
          </div>
          <div>
            <p class="text-white text-sm font-semibold">Dr. Arjun Patel</p>
            <p class="text-blue-300 text-xs">Patel Multi-Specialty Clinic, Ahmedabad</p>
          </div>
        </div>
      </div>
    </div>
  </div>
  `
})
export class RegisterComponent {
  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error   = signal<string | null>(null);
  readonly showPwd = signal(false);

  readonly form = this.fb.nonNullable.group({
    clinicName:     ['', [Validators.required]],
    adminFirstName: ['', [Validators.required]],
    adminLastName:  ['', [Validators.required]],
    email:          ['', [Validators.required, Validators.email]],
    phoneNumber:    [''],
    password:       ['', [Validators.required, Validators.minLength(8)]],
  });

  get f() { return this.form.controls; }

  togglePwd(): void { this.showPwd.set(!this.showPwd()); }

  readonly benefits = [
    { icon: 'schedule',        title: 'Smart appointment scheduling',  desc: 'Automated slot management, queue numbers, and patient reminders' },
    { icon: 'groups',          title: 'Multi-doctor management',       desc: 'Manage all your doctors, departments and availability in one place' },
    { icon: 'medical_services',title: 'Electronic Medical Records',    desc: 'Digital prescriptions, visit history and secure patient files' },
    { icon: 'receipt_long',    title: 'Billing & invoicing',           desc: 'Generate invoices with GST support and payment tracking' },
  ];

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);
    this.auth.registerClinic(this.form.getRawValue()).subscribe({
      next:  ()    => this.router.navigate(['/app/dashboard']),
      error: (err: { error?: { title?: string } }) => {
        this.error.set(err?.error?.title ?? 'Registration failed. Please try again.');
        this.loading.set(false);
      },
    });
  }
}

