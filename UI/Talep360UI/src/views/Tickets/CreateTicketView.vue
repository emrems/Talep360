<template>
  <div class="create-ticket-page page-container">
    <header class="page-header">
      <h2>Yeni Talep Oluştur</h2>
    </header>

    <div class="card">
      <form @submit.prevent="submitTicket">
        <div class="form-group">
          <label>Konu</label>
          <input v-model="form.title" type="text" class="form-control" placeholder="Örn: Yazıcı çalışmıyor" required />
        </div>

        <div class="form-row">
          <!-- Kategori backendde yok, şimdilik kaldırıyoruz veya UI'da tutup backend'e göndermiyoruz -->
          <!-- 
          <div class="form-group">
            <label>Kategori</label>
            <select v-model="form.category" class="form-control" required>
              <option value="">Seçiniz</option>
              <option value="Hardware">Donanım</option>
              <option value="Software">Yazılım</option>
              <option value="Network">Ağ/İnternet</option>
              <option value="Access">Erişim Yetkisi</option>
            </select>
          </div>
          -->
          
          <div class="form-group">
            <label>Öncelik</label>
            <select v-model="form.priority" class="form-control" required>
              <option :value="1">Düşük</option>
              <option :value="2">Orta</option>
              <option :value="3">Yüksek</option>
            </select>
          </div>
        </div>

        <div class="form-group">
          <label>Açıklama</label>
          <textarea v-model="form.description" class="form-control" rows="5" placeholder="Sorununuzu detaylı bir şekilde açıklayınız..." required></textarea>
        </div>

        <div class="form-group">
          <label>Dosya Ekle (Opsiyonel)</label>
          <input type="file" @change="handleFileUpload" multiple class="form-control" />
          <small class="text-muted mt-2">Not: Dosya yükleme özelliği henüz aktif değil.</small>
        </div>

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="$router.back()">İptal</button>
          <button type="submit" class="btn btn-primary" :disabled="loading">
            {{ loading ? 'Gönderiliyor...' : 'Talebi Oluştur' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, reactive, ref, computed } from 'vue';
import { useStore } from 'vuex';
import { useRouter } from 'vue-router';
import ticketService from '@/services/ticket.service';
import notificationService from '@/services/notification.service';

export default defineComponent({
  name: 'CreateTicketView',
  setup() {
    const store = useStore();
    const router = useRouter();
    const loading = ref(false);
    
    // Priority Enum: Low=1, Medium=2, High=3 (Backend varsayımı)
    const form = reactive({
      title: '',
      priority: 2,
      description: '',
      files: [] as File[]
    });

    const handleFileUpload = (event: Event) => {
      const target = event.target as HTMLInputElement;
      if (target.files) {
        form.files = Array.from(target.files);
      }
    };

    const submitTicket = async () => {
      if (loading.value) return; // Çift tıklama koruması (frontend)
      loading.value = true;
      
      // Basit UUID v4 üretici
      const uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
      });

      try {
        const user = store.getters['auth/getUser'];
        if (!user) {
            notificationService.error('Kullanıcı bilgisi bulunamadı, lütfen tekrar giriş yapın.');
            router.push('/login');
            return;
        }

        const ticketData = {
          title: form.title,
          description: form.description,
          priority: Number(form.priority),
          createdBy: Number(user.id || user.userId), // Backend int bekliyor
          tenantId: Number(user.tenantId) // Backend int bekliyor
        };

        // Idempotency Key ile gönder
        const response = await ticketService.createTicket(ticketData, uuid);

        if (response.isSuccess) {
          notificationService.success('Talep başarıyla oluşturuldu.');
          router.push('/');
        } else {
          notificationService.error(response.message || 'Talep oluşturulurken bir hata oluştu.');
        }
      } catch (error: any) {
        console.error('Create Ticket Error:', error);
        notificationService.error('Bir hata oluştu: ' + (error.message || 'Bilinmeyen hata'));
      } finally {
        // loading.value = false; // Başarılı olursa yönlendiriyoruz, hata olursa butonu tekrar aktif et
        // Ancak idempotency key kullanıldığı için aynı key ile tekrar denerse backend hata dönecek (duplicate).
        // Bu yüzden hata durumunda yeni key üretilmesi gerekebilir veya kullanıcı uyarılmalı.
        // Şimdilik sadece hata durumunda loading'i kapatıyoruz.
        if (loading.value) loading.value = false; 
      }
    };


    return {
      form,
      loading,
      handleFileUpload,
      submitTicket
    };
  }
});
</script>

<style scoped>
/* Scoped styles removed in favor of global styles */
</style>
