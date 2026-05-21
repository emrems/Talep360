import api from '@/services/api';

export interface CreateTenantDto {
  name: string;
  adminEmail: string;
  adminPassword: string;
  adminFullName: string;
}

export interface UpdateTenantDto {
  id: number;
  name: string;
  isActive: boolean;
}

export interface TenantDto {
  id: number;
  name: string;
  isActive: boolean;
  createdAtUtc: string;
}

class TenantService {
  async createTenant(data: CreateTenantDto) {
    // api instance handles token and base URL
    return await api.post('/api/Tenant/create', data);
  }

  async updateTenant(data: UpdateTenantDto) {
    return await api.put('/api/Tenant/update', data);
  }

  async deleteTenant(id: number) {
    return await api.delete(`/api/Tenant/delete/${id}`);
  }

  async getAllTenants() {
    // api instance handles token and base URL
    return await api.get('/api/Tenant/list');
  }

  async getTenantById(id: number) {
    return await api.get(`/api/Tenant/${id}`);
  }
}

export default new TenantService();