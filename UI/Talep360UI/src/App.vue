<template>
  <component :is="layout">
    <router-view />
  </component>
  <NotificationContainer />
  <ConfirmDialog />
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useRoute } from 'vue-router';
import MainLayout from '@/layouts/MainLayout.vue';
import AuthLayout from '@/layouts/AuthLayout.vue';
import NotificationContainer from '@/components/NotificationContainer.vue';
import ConfirmDialog from '@/components/ConfirmDialog.vue';

export default defineComponent({
  name: 'App',
  components: {
    MainLayout,
    AuthLayout,
    NotificationContainer,
    ConfirmDialog
  },
  setup() {
    const route = useRoute();
    
    // Rota meta verisinden layout'u al, yoksa varsayılan MainLayout
    const layout = computed(() => {
      return route.meta.layout || 'MainLayout';
    });

    return {
      layout
    };
  }
});
</script>

<style>
@import './assets/styles/global.css';

/* Global Reset & Base Styles */
* {
  box-sizing: border-box;
}
</style>