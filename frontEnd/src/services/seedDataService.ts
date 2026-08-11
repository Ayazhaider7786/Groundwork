import type { SeedUser } from '../types/seedData';
import { apiClient } from './apiClient';

export const seedDataService = {
  async getSeedUsers(): Promise<SeedUser[]> {
    const { data } = await apiClient.get<SeedUser[]>('/seeddata/users');
    return data;
  },
};
