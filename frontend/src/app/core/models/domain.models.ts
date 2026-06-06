export type AppointmentStatus =
  | 'Requested'
  | 'Approved'
  | 'Rejected'
  | 'Rescheduled'
  | 'CheckedIn'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  departmentId?: string;
  departmentName?: string;
  type: string;
  status: AppointmentStatus;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  symptoms?: string;
  notes?: string;
  queueNumber?: number;
  estimatedWaitMinutes?: number;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Doctor {
  id: string;
  fullName: string;
  specialization?: string;
  departmentId?: string;
  departmentName?: string;
  consultationFee: number;
  experienceYears: number;
  photoUrl?: string;
  isAcceptingAppointments: boolean;
}
