export type UserRole = number | "User" | "Admin" | string;

export type UserDto = {
  id: string;
  email: string;
  fullName?: string | null;
  role: UserRole;
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
};

export type TokenResponse = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType: string;
  user: UserDto;
};

export type SessionDto = {
  id: string;
  createdAt: string;
  expiresAt: string;
  userAgent?: string | null;
  ipAddress?: string | null;
  isActive: boolean;
};
