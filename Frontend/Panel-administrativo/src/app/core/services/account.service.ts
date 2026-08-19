import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface Profile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  emailConfirmed: boolean;
  roles: string[];
  createdAt: string;
  lastLoginAt?: string;
}

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  phone?: string;
}

export interface ChangeEmailResponse {
  message: string;
  confirmationUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class AccountService {
  private http = inject(HttpClient);

  getProfile() {
    return this.http.get<Profile>(`${environment.apiUrl}/account/profile`);
  }

  updateProfile(payload: UpdateProfilePayload) {
    return this.http.put<Profile>(`${environment.apiUrl}/account/profile`, payload);
  }

  changePassword(currentPassword: string, newPassword: string) {
    return this.http.post<{ message: string }>(`${environment.apiUrl}/account/change-password`, {
      currentPassword,
      newPassword,
    });
  }

  changeEmail(newEmail: string) {
    return this.http.post<ChangeEmailResponse>(`${environment.apiUrl}/account/change-email/request`, {
      newEmail,
    });
  }
}