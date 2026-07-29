export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  isActive: boolean;
  emailConfirmed: boolean;
  roles: string[];
  permissions: string[];
  createdAt: string;
  updatedAt?: string;
  lastLoginAt?: string;
}
