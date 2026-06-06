import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center p-4
                bg-gradient-to-br from-brand-600 via-brand-700 to-brand-900">
      <mat-card class="w-full max-w-md p-8 rounded-2xl shadow-2xl">
        <div class="text-center mb-6">
          <div class="text-3xl font-bold text-brand-700">MediConnect</div>
          <p class="text-slate-500 mt-1">Sign in to your clinic workspace</p>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="flex flex-col gap-2">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="username" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
          </mat-form-field>

          @if (error()) {
            <div class="text-red-600 text-sm mb-2">{{ error() }}</div>
          }

          <button mat-flat-button color="primary" class="h-12" [disabled]="loading()">
            @if (loading()) {
              <mat-spinner diameter="22"></mat-spinner>
            } @else {
              Sign In
            }
          </button>
        </form>

        <p class="text-center text-sm text-slate-500 mt-6">
          New clinic?
          <a routerLink="/register" class="text-brand-600 font-medium hover:underline">Create an account</a>
        </p>
        <p class="text-center text-xs text-slate-400 mt-4">
          Demo: admin&#64;democlinic.io / Admin&#64;123
        </p>
      </mat-card>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['admin@democlinic.io', [Validators.required, Validators.email]],
    password: ['Admin@123', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/app/dashboard']),
      error: (err) => {
        this.error.set(err?.error?.title ?? 'Invalid email or password.');
        this.loading.set(false);
      }
    });
  }
}
