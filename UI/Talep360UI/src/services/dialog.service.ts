import { reactive } from 'vue';

interface DialogState {
  isOpen: boolean;
  title: string;
  message: string;
  resolve: ((value: boolean) => void) | null;
}

const state = reactive<DialogState>({
  isOpen: false,
  title: '',
  message: '',
  resolve: null
});

const confirm = (title: string, message: string): Promise<boolean> => {
  state.title = title;
  state.message = message;
  state.isOpen = true;

  return new Promise((resolve) => {
    state.resolve = resolve;
  });
};

const close = (result: boolean) => {
  state.isOpen = false;
  if (state.resolve) {
    state.resolve(result);
    state.resolve = null;
  }
};

export default {
  state,
  confirm,
  close
};
