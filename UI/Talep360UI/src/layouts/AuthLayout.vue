<template>
  <div class="auth-layout">
    <div class="auth-container">
      <div class="auth-header">
        <h1>Talep360</h1>
        <p>Kurumsal Talep Yönetim Sistemi</p>
      </div>
      <slot></slot>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'AuthLayout'
});
</script>

<style scoped>
.auth-layout {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f4f6f8;
}

.auth-container {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

.auth-header {
  text-align: center;
  margin-bottom: 2rem;
}

.auth-header h1 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.auth-header p {
  color: #7f8c8d;
  font-size: 0.9rem;
}
</style>