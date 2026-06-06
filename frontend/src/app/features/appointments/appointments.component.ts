import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AppointmentService } from '../../core/services/appointment.service';
import { Appointment, AppointmentStatus } from '../../core/models/domain.models';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressBarModule
  ],
  template: `
    <mat-card class="rounded-xl overflow-hidden">
      <div class="flex items-center justify-between p-5 border-b border-slate-200 dark:border-slate-700">
        <h2 class="text-lg font-semibold">Appointments</h2>
        <span class="text-sm text-slate-500">{{ total() }} total</span>
      </div>

      @if (loading()) {
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      }

      <table mat-table [dataSource]="appointments()" class="w-full">
        <ng-container matColumnDef="patient">
          <th mat-header-cell *matHeaderCellDef>Patient</th>
          <td mat-cell *matCellDef="let a">{{ a.patientName }}</td>
        </ng-container>

        <ng-container matColumnDef="doctor">
          <th mat-header-cell *matHeaderCellDef>Doctor</th>
          <td mat-cell *matCellDef="let a">{{ a.doctorName }}</td>
        </ng-container>

        <ng-container matColumnDef="datetime">
          <th mat-header-cell *matHeaderCellDef>Date / Time</th>
          <td mat-cell *matCellDef="let a">
            {{ a.appointmentDate }} · {{ a.startTime }}
          </td>
        </ng-container>

        <ng-container matColumnDef="type">
          <th mat-header-cell *matHeaderCellDef>Type</th>
          <td mat-cell *matCellDef="let a">{{ a.type }}</td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let a">
            <span class="px-2.5 py-1 rounded-full text-xs font-medium" [class]="statusClass(a.status)">
              {{ a.status }}
            </span>
          </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let a">
            @if (a.status === 'Requested') {
              <button mat-button color="primary" (click)="setStatus(a, 'Approved')">Approve</button>
              <button mat-button color="warn" (click)="setStatus(a, 'Rejected')">Reject</button>
            } @else if (a.status === 'Approved') {
              <button mat-button (click)="setStatus(a, 'CheckedIn')">Check-in</button>
            } @else if (a.status === 'CheckedIn') {
              <button mat-button (click)="setStatus(a, 'Completed')">Complete</button>
            }
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="columns"></tr>
        <tr mat-row *matRowDef="let row; columns: columns"></tr>
      </table>

      @if (!loading() && appointments().length === 0) {
        <div class="p-10 text-center text-slate-400">No appointments found.</div>
      }
    </mat-card>
  `
})
export class AppointmentsComponent {
  private readonly service = inject(AppointmentService);

  readonly columns = ['patient', 'doctor', 'datetime', 'type', 'status', 'actions'];
  readonly appointments = signal<Appointment[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAppointments({ pageSize: 50 }).subscribe({
      next: (res) => {
        this.appointments.set(res.items);
        this.total.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  setStatus(a: Appointment, status: AppointmentStatus): void {
    this.service.updateStatus(a.id, { newStatus: status }).subscribe({
      next: () => this.load()
    });
  }

  statusClass(status: AppointmentStatus): string {
    const map: Record<AppointmentStatus, string> = {
      Requested: 'bg-amber-100 text-amber-700',
      Approved: 'bg-blue-100 text-blue-700',
      Rejected: 'bg-red-100 text-red-700',
      Rescheduled: 'bg-purple-100 text-purple-700',
      CheckedIn: 'bg-cyan-100 text-cyan-700',
      InProgress: 'bg-indigo-100 text-indigo-700',
      Completed: 'bg-emerald-100 text-emerald-700',
      Cancelled: 'bg-slate-200 text-slate-600',
      NoShow: 'bg-rose-100 text-rose-700'
    };
    return map[status] ?? 'bg-slate-100 text-slate-600';
  }
}
