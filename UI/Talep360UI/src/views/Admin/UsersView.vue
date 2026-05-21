<template>
  <div class="users-page page-container">
    <header class="page-header">
      <div class="header-content">
        <h2>Kullanıcı Yönetimi</h2>
        <p v-if="tenantName" class="subtitle">{{ tenantName }}</p>
      </div>
      <div class="header-actions">
        <button @click="openCreateModal" class="btn btn-primary">
          <i class="fas fa-plus"></i> Yeni Kullanıcı Ekle
        </button>
      </div>
    </header>

    <div class="table-container">
      <table class="table-corporate">
        <thead>
          <tr>
            <th>Ad Soyad</th>
            <th>Ünvan</th>
            <th>Email</th>
            <th>Rol</th>
            <th>Durum</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id">
            <td class="cell-title">{{ user.fullName }}</td>
            <td class="cell-desc">{{ user.title || '-' }}</td>
            <td class="cell-desc">{{ user.email }}</td>
            <td>
              <span v-for="role in user.roles" :key="role" :class="['badge', getRoleBadgeClass(role)]" style="margin-right: 4px;">
                {{ role }}
              </span>
            </td>
            <td>
              <span :class="['badge', user.isActive ? 'badge-success' : 'badge-danger']">
                {{ user.isActive ? 'Aktif' : 'Pasif' }}
              </span>
            </td>
            <td>
              <div class="actions-cell">
                <button @click="openEditModal(user)" class="action-btn edit" title="Düzenle">
                  ✏️
                </button>
                <button @click="handleDeleteUser(user)" class="action-btn delete" title="Sil">
                  🗑️
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="users.length === 0">
            <td colspan="6" class="empty-state">Henüz kullanıcı bulunmuyor.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create/Edit User Modal -->
    <div v-if="showCreateModal" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ editMode ? 'Kullanıcı Düzenle' : 'Yeni Kullanıcı Oluştur' }}</h3>
          <button @click="closeCreateModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="handleSubmit">
            <div class="form-group">
              <label>Ad Soyad</label>
              <input v-model="createForm.fullName" type="text" class="form-control" required placeholder="Örn: Ali Yılmaz" />
            </div>
            <div class="form-group">
              <label>Ünvan</label>
              <input v-model="createForm.title" type="text" class="form-control" placeholder="Örn: Yazılım Uzmanı" />
            </div>
            <div class="form-group">
              <label>E-Posta</label>
              <input v-model="createForm.email" type="email" class="form-control" required placeholder="ornek@sirket.com" :disabled="editMode" />
            </div>
            <div class="form-group" v-if="!editMode">
              <label>Şifre</label>
              <input v-model="createForm.password" type="password" class="form-control" required placeholder="******" />
            </div>
            <div class="form-group">
              <label>Roller (Birden fazla seçmek için Ctrl tuşuna basılı tutun)</label>
              <select v-model="createForm.roles" multiple required class="form-control multi-select">
                <option value="Admin">Admin (Yönetici)</option>
                <option value="Manager">Manager (Müdür)</option>
                <option value="Staff">Staff (Personel)</option>
                <option value="User">User (Kullanıcı)</option>
              </select>
            </div>
            <div class="form-group checkbox-group" v-if="editMode">
              <label>
                <input type="checkbox" v-model="createForm.isActive" />
                Aktif Kullanıcı
              </label>
            </div>

            <div class="error-message" v-if="errorMessage">{{ errorMessage }}</div>

            <div class="modal-footer">
              <button type="button" @click="closeCreateModal" class="btn btn-secondary">İptal</button>
              <button type="submit" class="btn btn-primary" :disabled="loading">
                {{ loading ? 'İşleniyor...' : (editMode ? 'Güncelle' : 'Oluştur') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, computed } from 'vue';
import { useStore } from 'vuex';
import UserService, { UserDto, CreateUserDto, UpdateUserDto } from '@/services/user.service';
import notificationService from '@/services/notification.service';
import dialogService from '@/services/dialog.service';

export default defineComponent({
  name: 'UsersView',
  setup() {
    const store = useStore();
    const users = ref<UserDto[]>([]);
    const showCreateModal = ref(false);
    const loading = ref(false);
    const errorMessage = ref('');
    
    const editMode = ref(false);
    const editId = ref(0);

    const tenantName = computed(() => {
        const user = store.getters['auth/getUser'];
        return user?.tenantName || user?.TenantName || '';
    });

    const createForm = ref({
      fullName: '',
      title: '',
      email: '',
      password: '',
      roles: [] as string[],
      isActive: true
    });

    const loadUsers = async () => {
      try {
        const response = await UserService.getTenantUsers();
        if (response.succeeded || response.isSuccess) {
          users.value = response.data;
        }
      } catch (error) {
        console.error('Kullanıcılar yüklenirken hata:', error);
      }
    };

    const handleSubmit = async () => {
      loading.value = true;
      errorMessage.value = '';

      try {
        let response;
        if (editMode.value) {
            const updateDto: UpdateUserDto = {
                id: editId.value,
                fullName: createForm.value.fullName,
                title: createForm.value.title,
                roles: createForm.value.roles,
                isActive: createForm.value.isActive
            };
            response = await UserService.updateUser(updateDto);
        } else {
            const createDto: CreateUserDto = {
                fullName: createForm.value.fullName,
                title: createForm.value.title,
                email: createForm.value.email,
                password: createForm.value.password,
                roles: createForm.value.roles
            };
            response = await UserService.createUser(createDto);
        }

        if (response.succeeded || response.isSuccess) {
          notificationService.success(editMode.value ? 'Kullanıcı güncellendi!' : 'Kullanıcı oluşturuldu!');
          closeCreateModal();
          loadUsers();
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

    const handleDeleteUser = async (user: UserDto) => {
        const confirmed = await dialogService.confirm('Kullanıcı Sil', `${user.fullName} adlı kullanıcıyı silmek istediğinize emin misiniz?`);
        if (!confirmed) return;

        try {
            const response = await UserService.deleteUser(user.id);
            if (response.succeeded || response.isSuccess) {
                notificationService.success('Kullanıcı silindi.');
                loadUsers();
            } else {
                notificationService.error(response.message || 'Silme başarısız.');
            }
        } catch (error: any) {
            notificationService.error(error.message || 'Hata oluştu.');
        }
    };

    const openCreateModal = () => {
      editMode.value = false;
      editId.value = 0;
      createForm.value = { fullName: '', title: '', email: '', password: '', roles: [], isActive: true };
      showCreateModal.value = true;
      errorMessage.value = '';
    };

    const openEditModal = (user: UserDto) => {
      editMode.value = true;
      editId.value = user.id;
      createForm.value = { 
          fullName: user.fullName, 
          title: user.title || '',
          email: user.email, 
          password: '', // Şifre güncellenmiyor bu formda
          roles: [...user.roles], 
          isActive: user.isActive 
      };
      showCreateModal.value = true;
      errorMessage.value = '';
    };

    const closeCreateModal = () => {
      showCreateModal.value = false;
    };

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
      loadUsers();
    });

    return {
      users,
      showCreateModal,
      createForm,
      loading,
      errorMessage,
      editMode,
      tenantName,
      handleSubmit,
      handleDeleteUser,
      openCreateModal,
      openEditModal,
      closeCreateModal,
      getRoleBadgeClass
    };
  }
});
</script>

<style scoped>
</style>
