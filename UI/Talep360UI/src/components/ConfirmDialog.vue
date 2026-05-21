<template>
  <transition name="fade">
    <div v-if="state.isOpen" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ state.title }}</h3>
          <button @click="cancel" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <p>{{ state.message }}</p>
        </div>
        <div class="modal-footer">
          <button @click="cancel" class="btn-secondary">İptal</button>
          <button @click="confirm" class="btn-primary">Onayla</button>
        </div>
      </div>
    </div>
  </transition>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import dialogService from '@/services/dialog.service';

export default defineComponent({
  name: 'ConfirmDialog',
  setup() {
    const state = computed(() => dialogService.state);

    const confirm = () => {
      dialogService.close(true);
    };

    const cancel = () => {
      dialogService.close(false);
    };

    return {
      state,
      confirm,
      cancel
    };
  }
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000; /* Higher than notifications */
  backdrop-filter: blur(2px);
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 400px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.2);
  transform: translateY(0);
  transition: transform 0.3s ease;
  overflow: hidden;
}

.modal-header {
  padding: 1.25rem 1.5rem;
  background-color: #f8f9fa;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.1rem;
  color: #2c3e50;
}

.modal-body {
  padding: 1.5rem;
  color: #4a5568;
  font-size: 1rem;
  line-height: 1.5;
}

.modal-footer {
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  border-top: 1px solid #eee;
}

.btn-primary {
  background-color: #3498db;
  color: white;
  border: none;
  padding: 0.6rem 1.25rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  font-size: 0.95rem;
  transition: background 0.2s;
}

.btn-primary:hover {
  background-color: #2980b9;
}

.btn-secondary {
  background-color: #ecf0f1;
  color: #576574;
  border: none;
  padding: 0.6rem 1.25rem;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  font-size: 0.95rem;
  transition: background 0.2s;
}

.btn-secondary:hover {
  background-color: #dfe6e9;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #95a5a6;
  line-height: 1;
}

.close-btn:hover {
  color: #7f8c8d;
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.fade-enter-from .modal-content,
.fade-leave-to .modal-content {
  transform: translateY(-20px);
}
</style>