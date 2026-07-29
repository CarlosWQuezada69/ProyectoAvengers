export interface Role {
  id: string;
  name: string;
  description?: string;
  permissionCount?: number;
  userCount?: number;
}
