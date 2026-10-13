export interface UserQueryParams {
  pageNumber: number;
  pageSize: number;
  searchText?: string;
  searchIn?: 'all' | 'username' | 'id';
  isBanned?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface AssignRoleRequest {
  roleName: string;
}

export interface UserActionResponse {
  success: boolean;
  message: string;
}

export interface PagedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
}
