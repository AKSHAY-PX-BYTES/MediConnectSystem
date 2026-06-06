export type UserRole = 'SuperAdmin' | 'ClinicAdmin' | 'Doctor' | 'Receptionist' | 'Patient';

export interface AppUser {
  id: string;
  tenantId: string | null;
  email: string;
  fullName: string;
  role: UserRole;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: AppUser;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterClinicRequest {
  clinicName: string;
  adminFirstName: string;
  adminLastName: string;
  email: string;
  password: string;
  phoneNumber?: string;
}
