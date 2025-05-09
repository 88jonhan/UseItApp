export interface User {
  id?: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  createdAt?: Date;

  isBlocked: boolean;
  blockReason: string;
  blockedUntil: Date;
}
