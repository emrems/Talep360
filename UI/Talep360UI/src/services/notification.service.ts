import { reactive } from 'vue';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface Notification {
  id: number;
  message: string;
  type: NotificationType;
  duration?: number;
}

const state = reactive({
  notifications: [] as Notification[]
});

let nextId = 1;

const remove = (id: number) => {
  const index = state.notifications.findIndex(n => n.id === id);
  if (index !== -1) {
    state.notifications.splice(index, 1);
  }
};

const notify = (message: string, type: NotificationType = 'info', duration: number = 3000) => {
  const id = nextId++;
  const notification: Notification = { id, message, type, duration };
  
  state.notifications.push(notification);

  if (duration > 0) {
    setTimeout(() => {
      remove(id);
    }, duration);
  }
};

export default {
  state,
  notify,
  remove,
  success: (msg: string, duration?: number) => notify(msg, 'success', duration),
  error: (msg: string, duration?: number) => notify(msg, 'error', duration),
  warning: (msg: string, duration?: number) => notify(msg, 'warning', duration),
  info: (msg: string, duration?: number) => notify(msg, 'info', duration),
};
