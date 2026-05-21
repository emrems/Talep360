import api from '@/services/api';

export interface CreateUserDto {
  email: string;
  password?: string;
  fullName: string;
  title?: string;
  roles: string[];
}

export interface UpdateUserDto {
  id: number;
  fullName: string;
  title?: string;
  roles: string[];
  isActive: boolean;
}

export interface UserDto {
  id: number;
  email: string;
  fullName: string;
  title?: string;
  roles: string[];
  isActive: boolean;
}

export interface BaseResponse<T> {
    succeeded: boolean;
    isSuccess: boolean;
    message: string;
    errors: string[] | null;
    data: T;
}

class UserService {
  async createUser(data: CreateUserDto): Promise<BaseResponse<boolean>> {
    return await api.post('/api/User/create', data);
  }

  async updateUser(data: UpdateUserDto): Promise<BaseResponse<boolean>> {
    return await api.put('/api/User/update', data);
  }

  async deleteUser(id: number): Promise<BaseResponse<boolean>> {
    return await api.delete(`/api/User/delete/${id}`);
  }

  async getTenantUsers(): Promise<BaseResponse<UserDto[]>> {
    return await api.get('/api/User/list');
  }

  async getTenantUsersByTenantId(tenantId: number): Promise<BaseResponse<UserDto[]>> {
    return await api.get(`/api/User/tenant/${tenantId}/users`);
  }
}

export default new UserService();
