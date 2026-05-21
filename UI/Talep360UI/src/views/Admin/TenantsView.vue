<template>
  <div class="tenants-page page-container">
    <header class="page-header">
      <div class="header-content">
        <h2>Şirket Yönetimi</h2>
        <p class="subtitle">Sistemdeki tüm şirketlerin yönetimi</p>
      </div>
      <div class="header-actions">
        <button @click="showCreateModal = true" class="btn btn-primary">
          <i class="fas fa-plus"></i> Yeni Şirket Ekle
        </button>
      </div>
    </header>

    <!-- Şirketler Listesi -->
    <div class="table-container">
      <table class="table-corporate">
        <thead>
          <tr>
            <th>ID</th>
            <th>Şirket Adı</th>
            <th>Durum</th>
            <th>Oluşturulma Tarihi</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="tenant in tenants" :key="tenant.id">
            <td class="id-cell">#{{ tenant.id }}</td>
            <td class="cell-title">{{ tenant.name }}</td>
            <td>
              <span :class="['badge', tenant.isActive ? 'badge-success' : 'badge-danger']">
                {{ tenant.isActive ? 'Aktif' : 'Pasif' }}
              </span>
            </td>
            <td class="date-cell">{{ new Date(tenant.createdAtUtc).toLocaleDateString('tr-TR') }}</td>
            <td>
              <div class="actions-cell">
                <button @click="$router.push(`/admin/tenants/${tenant.id}/users`)" class="action-btn info" title="Personel Listesi">
                  👥
                </button>
                <button @click="openEditModal(tenant)" class="action-btn edit" title="Düzenle">
                  ✏️
                </button>
                <button v-if="tenant.isActive" @click="handleDelete(tenant)" class="action-btn delete" title="Pasife Al">
                  🗑️
                </button>
                <button v-else @click="openEditModal(tenant)" class="action-btn success" title="Aktifleştir (Düzenle)">
                  🔄
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="tenants.length === 0">
            <td colspan="5" class="empty-state">Henüz kayıtlı şirket bulunmamaktadır.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Yeni Şirket Ekleme Modalı -->
    <div v-if="showCreateModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>Yeni Şirket Oluştur</h3>
          <button @click="closeCreateModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="handleCreateTenant">
            <div class="form-group">
              <label>Şirket Adı</label>
              <input v-model="createForm.name" type="text" class="form-control" required placeholder="Örn: ABC Lojistik" />
            </div>

            <div class="form-section-title">Yönetici Bilgileri</div>
            
            <div class="form-group">
              <label>Ad Soyad</label>
              <input v-model="createForm.adminFullName" type="text" class="form-control" required placeholder="Örn: Ahmet Yılmaz" />
            </div>

            <div class="form-group">
              <label>E-posta Adresi</label>
              <input v-model="createForm.adminEmail" type="email" class="form-control" required placeholder="admin@abclojistik.com" />
            </div>

            <div class="form-group">
              <label>Şifre</label>
              <input v-model="createForm.adminPassword" type="password" class="form-control" required placeholder="******" />
            </div>

            <div v-if="errorMessage" class="error-message">
              {{ errorMessage }}
            </div>

            <div class="modal-footer">
              <button type="button" @click="closeCreateModal" class="btn btn-secondary">İptal</button>
              <button type="submit" class="btn btn-primary" :disabled="loading">
                {{ loading ? 'Oluşturuluyor...' : 'Oluştur' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Şirket Düzenleme Modalı -->
    <div v-if="showEditModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>Şirket Düzenle</h3>
          <button @click="closeEditModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="handleUpdateTenant">
            <div class="form-group">
              <label>Şirket Adı</label>
              <input v-model="editForm.name" type="text" required />
            </div>

            <div class="form-group">
              <label>Durum</label>
              <select v-model="editForm.isActive" required>
                <option :value="true">Aktif</option>
                <option :value="false">Pasif</option>
              </select>
            </div>

            <div v-if="errorMessage" class="error-message">
              {{ errorMessage }}
            </div>

            <div class="modal-footer">
              <button type="button" @click="closeEditModal" class="btn-secondary">İptal</button>
              <button type="submit" class="btn-primary" :disabled="loading">
                {{ loading ? 'Güncelleniyor...' : 'Güncelle' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import TenantService, { TenantDto } from '@/services/tenant.service';
import notificationService from '@/services/notification.service';
import dialogService from '@/services/dialog.service';

export default defineComponent({
  name: 'TenantsView',
  setup() {
    const router = useRouter();
    const tenants = ref<TenantDto[]>([]);
    const showCreateModal = ref(false);
    const showEditModal = ref(false);
    const loading = ref(false);
    const errorMessage = ref('');

    const createForm = ref({
      name: '',
      adminEmail: '',
      adminPassword: '',
      adminFullName: ''
    });

    const editForm = ref({
      id: 0,
      name: '',
      isActive: true
    });

    const loadTenants = async () => {
      try {
        const response = await TenantService.getAllTenants();
        if (response.succeeded || response.isSuccess) {
          tenants.value = response.data;
        }
      } catch (error) {
        console.error('Şirketler yüklenirken hata:', error);
      }
    };

    const handleCreateTenant = async () => {
      loading.value = true;
      errorMessage.value = '';
      
      try {
        const response = await TenantService.createTenant(createForm.value);
        if (response.succeeded || response.isSuccess) {
          notificationService.success('Şirket başarıyla oluşturuldu!');
          closeCreateModal();
          loadTenants();
        } else {
          errorMessage.value = response.message || 'Bir hata oluştu.';
          notificationService.error(errorMessage.value);
        }
      } catch (error: any) {
        errorMessage.value = error.response?.data?.message || 'Beklenmedik bir hata oluştu.';
        notificationService.error(errorMessage.value);
      } finally {
        loading.value = false;
      }
    };

    const handleUpdateTenant = async () => {
      loading.value = true;
      errorMessage.value = '';

      try {
        const response = await TenantService.updateTenant(editForm.value);
        if (response.succeeded || response.isSuccess) {
          notificationService.success('Şirket bilgileri güncellendi!');
          closeEditModal();
          loadTenants();
        } else {
          errorMessage.value = response.message || 'Bir hata oluştu.';
          notificationService.error(errorMessage.value);
        }
      } catch (error: any) {
        errorMessage.value = error.response?.data?.message || 'Beklenmedik bir hata oluştu.';
        notificationService.error(errorMessage.value);
      } finally {
        loading.value = false;
      }
    };

    const handleDelete = async (tenant: TenantDto) => {
      const confirmed = await dialogService.confirm(
        'Pasife Al', 
        `${tenant.name} şirketini pasife almak istediğinize emin misiniz?`
      );
      
      if (!confirmed) {
        return;
      }

      try {
        const response = await TenantService.deleteTenant(tenant.id);
        if (response.succeeded || response.isSuccess) {
          notificationService.success('Şirket pasife alındı.');
          loadTenants();
        } else {
          notificationService.error(response.message || 'Bir hata oluştu.');
        }
      } catch (error: any) {
        notificationService.error(error.response?.data?.message || 'Beklenmedik bir hata oluştu.');
      }
    };

    const openEditModal = (tenant: TenantDto) => {
      editForm.value = {
        id: tenant.id,
        name: tenant.name,
        isActive: tenant.isActive
      };
      showEditModal.value = true;
      errorMessage.value = '';
    };

    const closeCreateModal = () => {
      showCreateModal.value = false;
      createForm.value = {
        name: '',
        adminEmail: '',
        adminPassword: '',
        adminFullName: ''
      };
      errorMessage.value = '';
    };

    const closeEditModal = () => {
      showEditModal.value = false;
      errorMessage.value = '';
    };

    onMounted(() => {
      loadTenants();
    });

    return {
      tenants,
      showCreateModal,
      showEditModal,
      createForm,
      editForm,
      loading,
      errorMessage,
      handleCreateTenant,
      handleUpdateTenant,
      handleDelete,
      openEditModal,
      closeCreateModal,
      closeEditModal
    };
  }
});
</script>

<style scoped>
</style>