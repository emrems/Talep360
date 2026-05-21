<template>
  <div class="notification-container">
    <transition-group name="toast">
      <div
        v-for="notification in notifications"
        :key="notification.id"
        class="toast-item"
        :class="notification.type"
        @click="remove(notification.id)"
      >
        <div class="icon">
          <span v-if="notification.type === 'success'">✅</span>
          <span v-else-if="notification.type === 'error'">❌</span>
          <span v-else-if="notification.type === 'warning'">⚠️</span>
          <span v-else>ℹ️</span>
        </div>
        <div class="message">{{ notification.message }}</div>
        <div class="close">&times;</div>
      </div>
    </transition-group>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import notificationService from '@/services/notification.service';

export default defineComponent({
  name: 'NotificationContainer',
  setup() {
    const notifications = computed(() => notificationService.state.notifications);
    const remove = notificationService.remove;

    return {
      notifications,
      remove
    };
  }
});
</script>

<style scoped>
.notification-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
  pointer-events: none; /* Allow clicks to pass through container */
}

.toast-item {
  pointer-events: auto; /* Enable clicks on toasts */
  min-width: 300px;
  max-width: 450px;
  padding: 16px;
  border-radius: 8px;
  background: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  border-left: 5px solid #ccc;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-size: 0.95rem;
  transition: all 0.3s ease;
}

/* Types */
.toast-item.success {
  border-left-color: #2e7d32;
  background-color: #f1f8e9;
}
.toast-item.error {
  border-left-color: #c62828;
  background-color: #ffebee;
}
.toast-item.warning {
  border-left-color: #fbc02d;
  background-color: #fffde7;
}
.toast-item.info {
  border-left-color: #1976d2;
  background-color: #e3f2fd;
}

.icon {
  font-size: 1.2rem;
}

.message {
  flex: 1;
  color: #333;
  line-height: 1.4;
}

.close {
  color: #999;
  font-size: 1.2rem;
  font-weight: bold;
}

/* Animations */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.4s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(30px);
}
</style>