import axios from 'axios';
import store from '@/store';

// Ocelot Gateway URL 
const API_URL = 'http://localhost:5063'; 

const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    }
});

// Request Interceptor: Her isteğe Token ekle
api.interceptors.request.use(
    config => {
        const token = store.getters['auth/getToken'];
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
);

// Response Interceptor: BaseResponse handle et ve hataları yakala
api.interceptors.response.use(
    response => {
        // Backend'den gelen yapı: { succeeded: true, data: ..., message: ... }
        // Eğer başarılıysa direkt response.data'yı veya response.data.data'yı dönebiliriz.
      
        return response.data; 
    },
    error => {
        if (error.response) {
            // 401 Unauthorized -> Logout
            if (error.response.status === 401) {
                store.dispatch('auth/logout');
                // Router push login sayfasına yönlendirilebilir (component içinden veya router.js'den yapmak daha sağlıklı)
            }
            
            // Backend'den gelen hata mesajını fırlat
            // Beklenen format: { succeeded: false, errors: [], message: "..." }
            const message = error.response.data?.message || error.response.data?.Message || 'Bir hata oluştu.';
            return Promise.reject(new Error(message));
        }
        return Promise.reject(error);
    }
);

export default api;