import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentService } from '../../core/services/appointment.service';
import { Appointment, AppointmentStatus } from '../../core/models/domain.models';

type FilterTab = 'All' | AppointmentStatus;

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="animate-fade-in">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-5">
      <div>
        <h2 class="text-xl font-bold text-slate-900">Appointments</h2>
        <p class="text-sm text-slate-500 mt-0.5">{{ total() }} total records</p>
      </div>
      <button class="mc-btn mc-btn--primary mc-btn--sm self-start" (click)="load()">
        <span class="material-symbols-outlined" style="font-size:1rem">refresh</span>
        Refresh
      </button>
    </div>

    <!-- Filter tabs -->
    <div class="flex flex-wrap gap-2 mb-4">
      @for (tab of tabs; track tab) {
        <button
          (click)="activeTab.set(tab)"
          class="px-3.5 py-1.5 rounded-full text-xs font-semibold border transition-all"
          [style]="activeTab() === tab
            ? 'background:#2563eb;color:#fff;border-color:#2563eb'
            : 'background:#fff;color:#64748b;border-color:#e2e8f0'">
          {{ tab }}
          @if (tab === 'All') { ({{ total() }}) }
          @else { ({{ countByStatus(tab) }}) }
        </button>
      }
    </div>

    <!-- Loading bar -->
    @if (loading()) {
      <div class="w-full h-1 rounded-full overflow-hidden mb-3" style="background:#e2e8f0">
        <div class="h-1 bg-brand-500 animate-pulse" style="width:60%"></div>
      </div>
    }

    <!-- Table -->
    <div class="mc-card p-0 overflow-x-auto">
      <table class="mc-table">
        <thead>
          <tr>
            <th>Patient</th>
            <th>Doctor</th>
            <th>Date / Time</th>
            <th>Type</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @if (filtered().length === 0 && !loading()) {
            <tr>
              <td colspan="6" class="text-center py-12 text-slate-400">
                <span class="material-symbols-outlined" style="font-size:2.5rem;display:block;margin-bottom:.5rem">event_busy</span>
                No appointments found.
              </td>
            </tr>
          }
          @for (a of filtered(); track a.id) {
            <tr>
              <td>
                <div class="font-medium text-slate-800">{{ a.patientName }}</div>
              </td>
              <td>
                <div class="text-slate-600">{{ a.doctorName }}</div>
              </td>
              <td>
                <div class="text-slate-700">{{ a.appointmentDate }}</div>
                <div class="text-xs text-slate-400">{{ a.startTime }}</div>
              </td>
              <td>
                <span class="text-slate-600 text-sm">{{ a.type }}</span>
              </td>
              <td>
                <span [class]="'mc-badge mc-badge--' + a.status.toLowerCase()">{{ a.status }}</span>
              </td>
              <td>
                <div class="flex items-center gap-1.5">
                  @if (a.status === 'Requested') {
                    <button class="mc-btn mc-btn--primary mc-btn--sm" (click)="setStatus(a, 'Approved')">Approve</button>
                    <button class="mc-btn mc-btn--danger mc-btn--sm" (click)="setStatus(a, 'Rejected')">Reject</button>
                  } @else if (a.status === 'Approved') {
                    <button class="mc-btn mc-btn--ghost mc-btn--sm" (click)="setStatus(a, 'CheckedIn')">Check-in</button>
                  } @else if (a.status === 'CheckedIn') {
                    <button class="mc-btn mc-btn--success mc-btn--sm" (click)="setStatus(a, 'InProgress')">Start</button>
                  } @else if (a.status === 'InProgress') {
                    <button class="mc-btn mc-btn--success mc-btn--sm" (click)="setStatus(a, 'Completed')">Complete</button>
                  } @else {
                    <span class="text-xs text-slate-400">—</span>
                  }
                </div>
              </td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  </div>
  `
})
export class AppointmentsComponent {
  private readonly service = inject(AppointmentService);

  readonly appointments = signal<Appointment[]>([]);
  readonly total        = signal(0);
  readonly loading      = signal(false);
  readonly activeTab    = signal<FilterTab>('All');

  readonly tabs: FilterTab[] = ['All', 'Requested', 'Approved', 'CheckedIn', 'InProgress', 'Completed', 'Cancelled'];

  readonly filtered = computed<Appointment[]>(() => {
    const tab = this.activeTab();
    return tab === 'All' ? this.appointments() : this.appointments().filter(a => a.status === tab);
  });

  countByStatus(status: FilterTab): number {
    return status === 'All' ? this.appointments().length
      : this.appointments().filter(a => a.status === status).length;
  }

  constructor() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.service.getAppointments({ pageSize: 100 }).subscribe({
      next: (res: { items: Appointment[]; totalCount: number }) => {
        this.appointments.set(res.items);
        this.total.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  setStatus(a: Appointment, status: AppointmentStatus): void {
    this.service.updateStatus(a.id, { newStatus: status }).subscribe({ next: () => this.load() });
  }
}

