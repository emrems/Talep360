<template>
  <div class="login-page">
    <div class="login-container">
      <div class="card login-card">
        <div class="text-center mb-4">
          <h2>Giriş Yap</h2>
          <p class="text-muted">Talep360 hesabınıza giriş yapın</p>
        </div>
        
        <form @submit.prevent="handleLogin">
          <div class="form-group">
            <label>Email Adresi</label>
            <input v-model="email" type="email" class="form-control" required placeholder="ornek@sirket.com" />
          </div>
          
          <div class="form-group">
            <label>Şifre</label>
            <input v-model="password" type="password" class="form-control" required placeholder="******" />
          </div>

          <div v-if="error" class="alert alert-danger">
            {{ error }}
          </div>

          <button type="submit" :disabled="loading" class="btn btn-primary btn-full">
            {{ loading ? 'Giriş Yapılıyor...' : 'Giriş Yap' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue';
import { useStore } from 'vuex';
import { useRouter } from 'vue-router';

export default defineComponent({
  name: 'LoginView',
  setup() {
    const store = useStore();
    const router = useRouter();
    
    const email = ref('');
    const password = ref('');
    const loading = ref(false);
    const error = ref('');

    const handleLogin = async () => {
      loading.value = true;
      error.value = '';
      
      try {
        await store.dispatch('auth/login', {
          email: email.value,
          password: password.value
        });
        
        // Başarılı giriş
        await router.push('/');
      } catch (err: any) {
        console.error('Login error:', err);
        error.value = err.message || 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.';
      } finally {
        loading.value = false;
      }
    };

    return {
      email,
      password,
      loading,
      error,
      handleLogin
    };
  }
});
</script>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: var(--bg-body);
  padding: 1rem;
}

.login-container {
  width: 100%;
  max-width: 720px;
}

.login-card {
  padding: 1.75rem;
}

.mb-4 {
  margin-bottom: 1rem;
}

.btn-full {
  width: 100%;
  margin-top: 1rem;
  padding: 0.8rem;
}
</style>
