import { User } from './user';

export interface Item {
  id?: number;
  name: string;
  description: string;
  category: string;
  imageUrl?: string;
  isAvailable: boolean;
  createdAt?: Date;
  ownerId: number;
  owner?: User;
}
