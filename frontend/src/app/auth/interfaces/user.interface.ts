export interface LoginResponse {
  user:    User;
  token:   string;
  message: string;
}

export interface User {
  id:          string;
  email:       string;
  userName:    string;
  displayName: string;
  createdAt: Date;
  roles:       string[];
  imageUrl: string|null;
  isBanned: boolean;
  bannedUntil?: string;
}
