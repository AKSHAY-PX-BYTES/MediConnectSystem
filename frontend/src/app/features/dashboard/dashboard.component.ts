import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AppointmentService } from '../../core/services/appointment.service';

interface StatCard {
  label: string;
  value: string;
  icon: string;
  accent: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
      @for (s of stats(); track s.label) {
        <mat-card class="p-5 rounded-xl">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-slate-500">{{ s.label }}</p>
              <p class="text-2xl font-bold mt-1">{{ s.value }}</p>
            </div>
            <div class="w-11 h-11 rounded-lg flex items-center justify-center" [class]="s.accent">
              <span class="material-symbols-outlined text-white">{{ s.icon }}</span>
            </div>
          </div>
        </mat-card>
      }
    </div>

    <mat-card class="p-6 rounded-xl">
      <h2 class="text-lg font-semibold mb-2">Today at a glance</h2>
      <p class="text-slate-500 text-sm">
        Manage appointments, doctor availability, patient records, prescriptions and billing —
        all isolated securely within your clinic tenant.
      </p>
    </mat-card>
  `
})
export class DashboardComponent {
  private readonly appointments = inject(AppointmentService);
  readonly stats = signal<StatCard[]>([
    { label: "Today's Appointments", value: '—', icon: 'event', accent: 'bg-brand-600' },
    { label: 'Pending Requests', value: '—', icon: 'pending_actions', accent: 'bg-amber-500' },
    { label: 'Completed', value: '—', icon: 'task_alt', accent: 'bg-emerald-500' },
    { label: 'Active Doctors', value: '—', icon: 'stethoscope', accent: 'bg-indigo-500' }
  ]);

  constructor() {
    this.loadStats();
  }

  private loadStats(): void {
    this.appointments.getAppointments({ pageSize: 1 }).subscribe({
      next: (res) => {
        this.stats.update((cards) => {
          const copy = [...cards];
          copy[0] = { ...copy[0], value: String(res.totalCount) };
          return copy;
        });
      },
      error: () => undefined
    });

    this.appointments.getAppointments({ status: 'Requested', pageSize: 1 }).subscribe({
      next: (res) => {
        this.stats.update((cards) => {
          const copy = [...cards];
          copy[1] = { ...copy[1], value: String(res.totalCount) };
          return copy;
        });
      },
      error: () => undefined
    });

    this.appointments.getDoctors().subscribe({
      next: (docs) => {
        this.stats.update((cards) => {
          const copy = [...cards];
          copy[3] = { ...copy[3], value: String(docs.length) };
          return copy;
        });
      },
      error: () => undefined
    });
  }
}
