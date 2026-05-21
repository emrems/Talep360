<template>
  <div class="home">
    <div class="loading-state">
      <div class="spinner"></div>
      Yönlendiriliyor...
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed, onMounted } from 'vue';
import { useStore } from 'vuex';
import { useRouter } from 'vue-router';

export default defineComponent({
  name: 'HomeView',
  setup() {
    const store = useStore();
    const router = useRouter();
    const userRoles = computed(() => store.getters['auth/getUserRoles']);

    onMounted(() => {
        const roles = userRoles.value || [];
        
        if (roles.includes('SuperAdmin')) {
            router.replace('/admin/tenants');
        } else if (roles.includes('Admin') || roles.includes('Manager')) {
            router.replace('/dashboard/manager');
        } else if (roles.includes('Staff') || roles.includes('User')) {
            router.replace('/dashboard/staff');
        } else {
            // Fallback
            console.warn('Rol bulunamadı veya eşleşmedi, staff paneline yönlendiriliyor.');
            router.replace('/dashboard/staff');
        }
    });

    return {};
  }
});
</script>

<style scoped>
.home {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: var(--bg-body);
}
</style>
