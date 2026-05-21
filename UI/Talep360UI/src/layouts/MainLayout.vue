<template>
  <div class="main-layout">
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="logo-area">
          <h3>Talep360</h3>
          <span class="version">v2.1</span>
        </div>
      </div>
      <nav class="sidebar-nav">
        <ul>
          <!-- Ortak Menüler -->
          <li>
            <router-link to="/" active-class="active">
              <span class="icon">🏠</span>
              <span class="text">Ana Sayfa</span>
            </router-link>
          </li>
          
          <!-- SuperAdmin Menüleri -->
          <template v-if="isSuperAdmin">
            <li class="menu-label">Yönetim</li>
            <li>
              <router-link to="/admin/tenants" active-class="active">
                <span class="icon">🏢</span>
                <span class="text">Şirketler</span>
              </router-link>
            </li>
            <li>
              <router-link to="/admin/settings" active-class="active">
                <span class="icon">⚙️</span>
                <span class="text">Sistem Ayarları</span>
              </router-link>
            </li>
          </template>

          <!-- Tenant Admin Menüleri -->
          <template v-if="isTenantAdmin">
             <li class="menu-label">Şirket Yönetimi</li>
             <li>
               <router-link to="/users" active-class="active">
                 <span class="icon">👥</span>
                 <span class="text">Kullanıcılar</span>
               </router-link>
             </li>
             <!-- <li><router-link to="/reports">📊 Raporlar</router-link></li> -->
          </template>

          <!-- Genel Menüler -->
          <li class="menu-label">Talepler</li>
          <li>
            <router-link to="/tickets/my-tickets" active-class="active">
              <span class="icon">🎫</span>
              <span class="text">Taleplerim</span>
            </router-link>
          </li>
          <li>
            <router-link to="/tickets/create" active-class="active">
              <span class="icon">➕</span>
              <span class="text">Yeni Talep</span>
            </router-link>
          </li>
        </ul>
      </nav>
      <div class="sidebar-footer">
        <div class="user-mini-profile">
          <div class="avatar-circle">{{ userInitials }}</div>
          <div class="user-details">
             <span class="user-name">{{ user?.fullName || user?.userName }}</span>
             <span class="user-role">{{ userRoles[0] }}</span>
          </div>
        </div>
        <button @click="logout" class="logout-btn" title="Çıkış Yap">
          🚪
        </button>
      </div>
    </aside>

    <main class="content">
      <header class="top-bar">
        <div class="breadcrumb">
          <!-- Basit Breadcrumb -->
          <span>{{ currentRouteName }}</span>
        </div>
        <div class="top-bar-actions">
           <!-- Bildirim İkonu Gelebilir -->
        </div>
      </header>
      <div class="page-content">
        <slot></slot>
      </div>
    </main>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useStore } from 'vuex';
import { useRouter, useRoute } from 'vue-router';

export default defineComponent({
  name: 'MainLayout',
  setup() {
    const store = useStore();
    const router = useRouter();
    const route = useRoute();

    const user = computed(() => store.getters['auth/getUser']);
    const userRoles = computed(() => store.getters['auth/getUserRoles']);
    const isSuperAdmin = computed(() => store.getters['auth/isSuperAdmin']);
    const isTenantAdmin = computed(() => store.getters['auth/isTenantAdmin']);

    const userInitials = computed(() => {
        const name = user.value?.fullName || user.value?.userName || '?';
        return name.substring(0, 2).toUpperCase();
    });

    const currentRouteName = computed(() => {
        // Rota adına göre başlık gösterme (Basitçe)
        const name = route.name as string;
        // Mapping yapılabilir ama şimdilik name'i dönelim
        return name || 'Panel';
    });

    const logout = async () => {
      await store.dispatch('auth/logout');
      router.push('/login');
    };

    return {
      user,
      userRoles,
      userInitials,
      isSuperAdmin,
      isTenantAdmin,
      currentRouteName,
      logout
    };
  }
});
</script>

<style scoped>
.main-layout {
  display: flex;
  height: 100vh;
  background-color: var(--bg-body);
}

/* Sidebar Styles */
.sidebar {
  width: var(--sidebar-width);
  background-color: var(--bg-sidebar);
  color: #ecf0f1;
  display: flex;
  flex-direction: column;
  box-shadow: 2px 0 5px rgba(0,0,0,0.1);
  transition: width 0.3s;
  z-index: 100;
}

.sidebar-header {
  height: var(--header-height);
  display: flex;
  align-items: center;
  padding: 0 1.5rem;
  background-color: rgba(0,0,0,0.1);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.logo-area {
    display: flex;
    align-items: baseline;
    gap: 0.5rem;
}

.sidebar-header h3 {
  margin: 0;
  color: white;
  font-weight: 700;
  letter-spacing: 1px;
}

.version {
    font-size: 0.7rem;
    color: var(--text-muted);
    opacity: 0.6;
}

.sidebar-nav {
  flex: 1;
  padding: 1rem 0;
  overflow-y: auto;
}

.sidebar-nav ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.sidebar-nav li a {
  display: flex;
  align-items: center;
  padding: 0.8rem 1.5rem;
  color: #a0aec0;
  text-decoration: none;
  transition: all 0.2s;
  border-left: 3px solid transparent;
}

.sidebar-nav li a:hover {
  background-color: var(--bg-sidebar-hover);
  color: white;
}

.sidebar-nav li a.active {
  background-color: rgba(0, 86, 179, 0.2);
  color: white;
  border-left-color: var(--accent-color);
}

.icon {
    margin-right: 0.8rem;
    font-size: 1.1rem;
    width: 24px;
    text-align: center;
}

.menu-label {
  padding: 1.5rem 1.5rem 0.5rem;
  font-size: 0.75rem;
  text-transform: uppercase;
  color: #64748b;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.sidebar-footer {
  padding: 1rem;
  background-color: rgba(0,0,0,0.2);
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.user-mini-profile {
    display: flex;
    align-items: center;
    gap: 0.8rem;
    overflow: hidden;
}

.avatar-circle {
    width: 36px;
    height: 36px;
    background-color: var(--primary-color);
    color: white;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    font-size: 0.9rem;
}

.user-details {
    display: flex;
    flex-direction: column;
}

.user-name {
    font-size: 0.9rem;
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 120px;
}

.user-role {
    font-size: 0.75rem;
    color: #94a3b8;
}

.logout-btn {
  background: none;
  border: none;
  color: #ef4444;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.logout-btn:hover {
  background-color: rgba(239, 68, 68, 0.1);
}

/* Content Styles */
.content {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.top-bar {
  height: var(--header-height);
  background: white;
  padding: 0 2rem;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.breadcrumb {
    font-size: 1.1rem;
    font-weight: 600;
    color: var(--text-primary);
}

.page-content {
  flex: 1;
  padding: 2rem;
  overflow-y: auto;
  background-color: var(--bg-body);
}
</style>