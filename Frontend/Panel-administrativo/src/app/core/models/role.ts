export interface RoleUser {
  id: string;
  name: string;
  email: string;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
  hierarchyLevel?: number;
  permissionCount?: number;
  userCount?: number;
  users?: RoleUser[];
}
