import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../core/services/appointment.service';

interface StatCard {
  label: string;
  value: string;
  icon: string;
  bg: string;
  iconColor: string;
  trend?: string;
  trendUp?: boolean;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
  <div class="animate-fade-in">
    <!-- Page heading -->
    <div class="mb-6">
      <h2 class="text-xl font-bold text-slate-900">Dashboard</h2>
      <p class="text-sm text-slate-500 mt-0.5">Overview of your clinic's activity</p>
    </div>

    <!-- Stat cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
      @for (s of stats(); track s.label) {
        <div class="mc-card flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0" [style]="s.bg">
            <span class="material-symbols-outlined" [style]="s.iconColor" style="font-size:1.4rem">{{ s.icon }}</span>
          </div>
          <div class="min-w-0">
            <p class="text-xs text-slate-500 font-medium">{{ s.label }}</p>
            <p class="text-2xl font-bold text-slate-900 mt-0.5">{{ s.value }}</p>
            @if (s.trend) {
              <p class="text-xs mt-0.5" [class]="s.trendUp ? 'text-emerald-600' : 'text-red-500'">
                {{ s.trend }}
              </p>
            }
          </div>
        </div>
      }
    </div>

    <!-- Quick actions + info row -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Quick actions -->
      <div class="mc-card lg:col-span-1">
        <h3 class="text-sm font-semibold text-slate-800 mb-4">Quick Actions</h3>
        <div class="space-y-2.5">
          <a routerLink="/app/appointments"
             class="flex items-center gap-3 p-3 rounded-xl bg-brand-50 hover:bg-brand-100 transition-colors cursor-pointer"
             style="text-decoration:none">
            <span class="material-symbols-outlined text-brand-600" style="font-size:1.1rem">event</span>
            <span class="text-sm font-medium text-brand-700">View all appointments</span>
          </a>
          <div class="flex items-center gap-3 p-3 rounded-xl bg-amber-50 hover:bg-amber-100 transition-colors cursor-pointer">
            <span class="material-symbols-outlined text-amber-600" style="font-size:1.1rem">pending_actions</span>
            <span class="text-sm font-medium text-amber-700">Review pending requests</span>
          </div>
          <div class="flex items-center gap-3 p-3 rounded-xl bg-emerald-50 hover:bg-emerald-100 transition-colors cursor-pointer">
            <span class="material-symbols-outlined text-emerald-600" style="font-size:1.1rem">person_add</span>
            <span class="text-sm font-medium text-emerald-700">Register new patient</span>
          </div>
        </div>
      </div>

      <!-- Info card -->
      <div class="mc-card lg:col-span-2 flex flex-col justify-between">
        <div>
          <h3 class="text-sm font-semibold text-slate-800 mb-1">Platform highlights</h3>
          <p class="text-sm text-slate-500 leading-relaxed">
            Your clinic data is fully isolated through multi-tenant architecture. Each doctor,
            appointment, patient, and prescription belongs exclusively to your workspace.
          </p>
        </div>
        <div class="grid grid-cols-3 gap-3 mt-5">
          @for (f of features; track f.label) {
            <div class="text-center p-3 rounded-xl bg-slate-50">
              <span class="material-symbols-outlined text-brand-600 mb-1" style="font-size:1.3rem">{{ f.icon }}</span>
              <p class="text-xs text-slate-600 font-medium">{{ f.label }}</p>
            </div>
          }
        </div>
      </div>
    </div>
  </div>
  `
})
export class DashboardComponent {
  private readonly appointmentService = inject(AppointmentService);

  readonly stats = signal<StatCard[]>([
    { label: "Today's Appointments", value: '—', icon: 'event',            bg: 'background:#eff6ff', iconColor: 'color:#2563eb' },
    { label: 'Pending Requests',      value: '—', icon: 'pending_actions', bg: 'background:#fffbeb', iconColor: 'color:#d97706' },
    { label: 'Completed Today',       value: '—', icon: 'task_alt',        bg: 'background:#f0fdf4', iconColor: 'color:#16a34a' },
    { label: 'Active Doctors',        value: '—', icon: 'stethoscope',     bg: 'background:#f5f3ff', iconColor: 'color:#7c3aed' },
  ]);

  readonly features = [
    { icon: 'lock',             label: 'Tenant isolation'    },
    { icon: 'notifications',    label: 'Smart reminders'     },
    { icon: 'receipt_long',     label: 'Billing & invoicing' },
  ];

  constructor() { this.loadStats(); }

  private loadStats(): void {
    this.appointmentService.getAppointments({ pageSize: 1 }).subscribe({
      next: (res) => this.patchStat(0, String(res.totalCount)),
      error: () => undefined,
    });
    this.appointmentService.getAppointments({ status: 'Requested', pageSize: 1 }).subscribe({
      next: (res) => this.patchStat(1, String(res.totalCount)),
      error: () => undefined,
    });
    this.appointmentService.getAppointments({ status: 'Completed', pageSize: 1 }).subscribe({
      next: (res) => this.patchStat(2, String(res.totalCount)),
      error: () => undefined,
    });
    this.appointmentService.getDoctors().subscribe({
      next: (docs) => this.patchStat(3, String(docs.length)),
      error: () => undefined,
    });
  }

  private patchStat(index: number, value: string): void {
    this.stats.update((cards) => cards.map((c, i) => i === index ? { ...c, value } : c));
  }
}

