<template>
  <div class="tenant-users-page page-container">
    <!-- Breadcrumb & Header -->
    <header class="page-header">
        <div class="header-content">
            <div class="breadcrumbs">
                <router-link to="/admin/tenants" class="breadcrumb-link">Şirketler</router-link>
                <span class="separator">/</span>
                <span class="current">{{ tenantName || 'Yükleniyor...' }}</span>
            </div>
            <h2 class="">Personel Listesi</h2>
            <p class="subtitle">{{ tenantName }} şirketine ait tüm çalışanların detaylı listesi ve iş yükü analizi.</p>
        </div>
        <div class="header-actions">
            <button @click="$router.push('/admin/tenants')" class="btn btn-secondary">
                ⬅️ Geri Dön
            </button>
        </div>
    </header>

    <!-- Stats Cards -->
    <div class="stats-grid">
        <div class="stat-card horizontal">
            <div class="stat-icon users">👥</div>
            <div class="stat-info">
                <span class="stat-label">Toplam Personel</span>
                <span class="stat-value">{{ users.length }}</span>
            </div>
        </div>
        <div class="stat-card horizontal">
            <div class="stat-icon workload">📊</div>
            <div class="stat-info">
                <span class="stat-label">Toplam Aktif İş</span>
                <span class="stat-value">{{ totalWorkload }}</span>
            </div>
        </div>
        <div class="stat-card horizontal">
            <div class="stat-icon active">✅</div>
            <div class="stat-info">
                <span class="stat-label">Aktif Kullanıcı</span>
                <span class="stat-value">{{ activeUserCount }}</span>
            </div>
        </div>
    </div>

    <!-- Content -->
    <div class="table-container">
        <div v-if="loading" class="loading-state">
            <div class="spinner"></div>
            <span>Veriler yükleniyor...</span>
        </div>

        <div v-else-if="users.length === 0" class="empty-state">
            <p>Bu şirkete ait kayıtlı personel bulunmamaktadır.</p>
        </div>

        <table v-else class="table-corporate">
            <thead>
                <tr>
                    <th>Ad Soyad</th>
                    <th>Ünvan</th>
                    <th>E-Posta</th>
                    <th>Roller</th>
                    <th>Durum</th>
                    <th>Aktif İş Yükü</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="user in users" :key="user.id">
                    <td>
                        <div class="user-cell">
                            <div class="avatar-circle">{{ getInitials(user.fullName) }}</div>
                            <span class="cell-title">{{ user.fullName }}</span>
                        </div>
                    </td>
                    <td class="cell-desc">{{ user.title || '-' }}</td>
                    <td class="cell-desc">{{ user.email }}</td>
                    <td>
                        <div class="roles-wrapper">
                            <span v-for="role in user.roles" :key="role" :class="['badge', getRoleBadgeClass(role)]">
                                {{ role }}
                            </span>
                        </div>
                    </td>
                    <td>
                        <span :class="['badge', user.isActive ? 'badge-success' : 'badge-danger']">
                            {{ user.isActive ? 'Aktif' : 'Pasif' }}
                        </span>
                    </td>
                    <td>
                        <div class="workload-cell">
                            <div class="progress-bar-bg">
                                <div class="progress-bar-fill" :style="{ width: Math.min(user.workload * 10, 100) + '%', backgroundColor: getWorkloadColor(user.workload) }"></div>
                            </div>
                            <span class="workload-text">{{ user.workload || 0 }} Talep</span>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import tenantService from '@/services/tenant.service';
import userService from '@/services/user.service';
import ticketService from '@/services/ticket.service';
import notificationService from '@/services/notification.service';

export default defineComponent({
  name: 'TenantUsersView',
  setup() {
    const route = useRoute();
    const router = useRouter();
    const tenantId = Number(route.params.id);
    
    const tenantName = ref('');
    const users = ref<any[]>([]);
    const loading = ref(true);

    const loadData = async () => {
        loading.value = true;
        try {
            // 1. Fetch Tenant Details
            const tenantResponse = await tenantService.getTenantById(tenantId);
            if (tenantResponse.succeeded || (tenantResponse as any).isSuccess) {
                tenantName.value = (tenantResponse as any).data.name;
            } else {
                notificationService.error('Şirket bilgileri alınamadı.');
                router.push('/admin/tenants');
                return;
            }

            // 2. Fetch Users
            const usersResponse = await userService.getTenantUsersByTenantId(tenantId);
            if (usersResponse.succeeded || (usersResponse as any).isSuccess) {
                let userList = (usersResponse as any).data;
                
                // 3. Fetch Workload
                const userIds = userList.map((u: any) => u.id);
                if (userIds.length > 0) {
                    try {
                        const workloadResponse = await ticketService.getWorkloadStats(userIds);
                        const workloadData = (workloadResponse as any).data || workloadResponse;
                        
                        if (Array.isArray(workloadData)) {
                            userList = userList.map((u: any) => {
                                const w = workloadData.find((x: any) => x.userId === u.id);
                                return { ...u, workload: w ? w.activeTicketCount : 0 };
                            });
                        }
                    } catch (err) {
                        console.error('İş yükü verisi alınamadı', err);
                    }
                }
                
                users.value = userList;
            } else {
                notificationService.error('Kullanıcı listesi alınamadı.');
            }
        } catch (error) {
            console.error(error);
            notificationService.error('Veriler yüklenirken bir hata oluştu.');
        } finally {
            loading.value = false;
        }
    };

    const getInitials = (name: string) => {
        if (!name) return '';
        return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    };

    const getWorkloadColor = (count: number) => {
        if (count === 0) return '#bdc3c7';
        if (count < 3) return '#2ecc71';
        if (count < 6) return '#f1c40f';
        return '#e74c3c';
    };

    const totalWorkload = computed(() => {
        return users.value.reduce((sum, user) => sum + (user.workload || 0), 0);
    });

    const activeUserCount = computed(() => {
        return users.value.filter(u => u.isActive).length;
    });

    const getRoleBadgeClass = (role: string) => {
        const map: Record<string, string> = {
            'Admin': 'badge-info',
            'Manager': 'badge-success',
            'Staff': 'badge-warning',
            'User': 'badge-secondary'
        };
        return map[role] || 'badge-secondary';
    };

    onMounted(() => {
        if (!tenantId) {
            router.push('/admin/tenants');
            return;
        }
        loadData();
    });

    return {
        tenantName,
        users,
        loading,
        getInitials,
        getWorkloadColor,
        getRoleBadgeClass,
        totalWorkload,
        activeUserCount
    };
  }
});
</script>

<style scoped>
</style>
