import api from '@/services/api';
import type { ActionContext } from 'vuex';

// Define types
export interface User {
    id: string;
    fullName: string;
    userName: string;
    email: string;
    tenantId: number;
    [key: string]: any;
}

export interface AuthState {
    user: User | null;
    token: string;
    roles: string[];
    isAuthenticated: boolean;
}

interface BaseResponse<T> {
    succeeded: boolean;
    data: T;
    message: string;
    errors: string[];
}

interface LoginResponse {
    jwToken: string;
    roles: string[];
    [key: string]: any; // other user data merged
}

const state: AuthState = {
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    token: localStorage.getItem('token') || '',
    roles: JSON.parse(localStorage.getItem('roles') || '[]'),
    isAuthenticated: !!localStorage.getItem('token')
};

const getters = {
    isAuthenticated: (state: AuthState) => state.isAuthenticated,
    getUser: (state: AuthState) => state.user,
    getToken: (state: AuthState) => state.token,
    getUserRoles: (state: AuthState) => state.roles,
    isSuperAdmin: (state: AuthState) => state.roles.includes('SuperAdmin'),
    isTenantAdmin: (state: AuthState) => state.roles.includes('Admin'),
    isStaff: (state: AuthState) => state.roles.includes('Staff'),
    isUser: (state: AuthState) => state.roles.includes('User')
};

const actions = {
    async login({ commit }: ActionContext<AuthState, any>, credentials: any) {
        try {
            // Because of interceptor, response is actually the body (BaseResponse)
            // We use 'unknown' cast because axios types think it returns AxiosResponse
            const response = await api.post('/api/Auth/login', credentials) as any;
            
            console.log('API Response:', response);
            // Backend'den dönen response.isSuccess true ise veya response.succeeded true ise
            if (response.succeeded || response.isSuccess) {
                console.log('Login successful, processing data...');
                // API'den dönen veriyi kontrol et
                const data = response.data; 
                const accessToken = data.accessToken || data.jwToken || data.token; // Her ihtimali dene
                const roles = data.roles || [];
                const userData = { ...data };
                
                // Token ve rol bilgilerini kullanıcı objesinden temizle (local storage'a temiz kaydetmek için)
                delete userData.accessToken;
                delete userData.jwToken;
                delete userData.token;
                delete userData.roles;
                
                if (!accessToken) {
                    console.error('Token not found in response data:', data);
                    // Eğer data içinde yoksa belki direkt response içindedir? (Eski yapı vs.)
                    if (response.jwToken) {
                         // Fallback
                         commit('SET_TOKEN', response.jwToken);
                         // ... diğerleri
                         return true;
                    }
                    throw new Error('Token alınamadı!');
                }

                commit('SET_TOKEN', accessToken);
                commit('SET_USER', userData);
                commit('SET_ROLES', roles);
                
                return true;
            } else {
                throw new Error(response.message || 'Giriş başarısız.');
            }
        } catch (error) {
            throw error;
        }
    },
    logout({ commit }: ActionContext<AuthState, any>) {
        commit('LOGOUT');
        // Sayfayı yenile veya login'e at (Router tarafında handle edilecek)
        window.location.href = '/login';
    }
};

const mutations = {
    SET_TOKEN(state: AuthState, token: string) {
        state.token = token;
        state.isAuthenticated = true;
        localStorage.setItem('token', token);
    },
    SET_USER(state: AuthState, user: User) {
        state.user = user;
        localStorage.setItem('user', JSON.stringify(user));
    },
    SET_ROLES(state: AuthState, roles: string[]) {
        state.roles = roles;
        localStorage.setItem('roles', JSON.stringify(roles));
    },
    LOGOUT(state: AuthState) {
        state.user = null;
        state.token = '';
        state.roles = [];
        state.isAuthenticated = false;
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        localStorage.removeItem('roles');
    }
};

export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
};
