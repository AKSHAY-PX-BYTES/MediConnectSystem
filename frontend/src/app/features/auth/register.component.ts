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
  selector: 'app-register',
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
      <mat-card class="w-full max-w-lg p-8 rounded-2xl shadow-2xl">
        <div class="text-center mb-6">
          <div class="text-3xl font-bold text-brand-700">Start your free trial</div>
          <p class="text-slate-500 mt-1">14 days free on the Starter plan — no card required</p>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
          <mat-form-field appearance="outline" class="sm:col-span-2">
            <mat-label>Clinic name</mat-label>
            <input matInput formControlName="clinicName" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>First name</mat-label>
            <input matInput formControlName="adminFirstName" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Last name</mat-label>
            <input matInput formControlName="adminLastName" />
          </mat-form-field>

          <mat-form-field appearance="outline" class="sm:col-span-2">
            <mat-label>Work email</mat-label>
            <input matInput type="email" formControlName="email" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Phone (optional)</mat-label>
            <input matInput formControlName="phoneNumber" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" />
          </mat-form-field>

          @if (error()) {
            <div class="text-red-600 text-sm mb-2 sm:col-span-2">{{ error() }}</div>
          }

          <button mat-flat-button color="primary" class="h-12 sm:col-span-2" [disabled]="loading()">
            @if (loading()) {
              <mat-spinner diameter="22"></mat-spinner>
            } @else {
              Create clinic account
            }
          </button>
        </form>

        <p class="text-center text-sm text-slate-500 mt-6">
          Already have an account?
          <a routerLink="/login" class="text-brand-600 font-medium hover:underline">Sign in</a>
        </p>
      </mat-card>
    </div>
  `
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    clinicName: ['', [Validators.required]],
    adminFirstName: ['', [Validators.required]],
    adminLastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    this.auth.registerClinic(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/app/dashboard']),
      error: (err) => {
        this.error.set(err?.error?.title ?? 'Registration failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
