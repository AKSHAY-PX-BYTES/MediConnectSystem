import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Appointment, AppointmentStatus, Doctor, PagedResult } from '../models/domain.models';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly baseUrl = `${environment.apiUrl}/appointments`;
  private readonly doctorsUrl = `${environment.apiUrl}/doctors`;

  constructor(private readonly http: HttpClient) {}

  getAppointments(filters: {
    doctorId?: string;
    patientId?: string;
    status?: AppointmentStatus;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<Appointment>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params = params.set(key, String(value));
      }
    });
    return this.http.get<PagedResult<Appointment>>(this.baseUrl, { params });
  }

  book(payload: {
    patientId: string;
    doctorId: string;
    departmentId?: string;
    type: string;
    appointmentDate: string;
    startTime: string;
    symptoms?: string;
  }): Observable<Appointment> {
    return this.http.post<Appointment>(this.baseUrl, payload);
  }

  updateStatus(
    id: string,
    body: { newStatus: AppointmentStatus; reason?: string; newDate?: string; newStartTime?: string }
  ): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/status`, body);
  }

  getDoctors(search?: string): Observable<Doctor[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<Doctor[]>(this.doctorsUrl, { params });
  }
}
