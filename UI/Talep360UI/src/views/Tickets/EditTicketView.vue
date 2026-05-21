<template>
  <div class="edit-ticket-page page-container">
    <header class="page-header">
      <h2>Talebi Düzenle</h2>
    </header>

    <div v-if="loadingData" class="loading-state">
        <div class="spinner"></div>
        Veriler yükleniyor...
    </div>

    <div v-else class="card">
      <div v-if="isReadOnly" class="alert alert-warning mb-2">
        ⚠️ Bu talep çözüldüğü için üzerinde değişiklik yapılamaz.
      </div>

      <form @submit.prevent="submitUpdate">
        <div class="form-group">
          <label>Konu</label>
          <input v-model="form.title" type="text" class="form-control" placeholder="Örn: Yazıcı çalışmıyor" required :disabled="isReadOnly" />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Öncelik</label>
            <select v-model="form.priority" class="form-control" required :disabled="isReadOnly">
              <option :value="1">Düşük</option>
              <option :value="2">Orta</option>
              <option :value="3">Yüksek</option>
            </select>
          </div>

          <div class="form-group" v-if="!isReadOnly">
            <label>Durum</label>
             <select v-model="form.status" class="form-control" required>
                <option value="Open">Açık</option>
                <option value="InProgress">İşlemde</option>
                <option value="Resolved">Çözüldü</option>
                <option value="Closed">Kapandı</option>
             </select>
          </div>
        </div>

        <div class="form-group">
          <label>Açıklama</label>
          <textarea v-model="form.description" class="form-control" rows="5" placeholder="Sorununuzu detaylı bir şekilde açıklayınız..." required :disabled="isReadOnly"></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" @click="$router.back()">{{ isReadOnly ? 'Geri Dön' : 'İptal' }}</button>
          <button type="submit" class="btn btn-primary" :disabled="loading" v-if="!isReadOnly">
            {{ loading ? 'Güncelleniyor...' : 'Güncelle' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, reactive, ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useStore } from 'vuex';
import ticketService from '@/services/ticket.service';
import notificationService from '@/services/notification.service';

export default defineComponent({
  name: 'EditTicketView',
  setup() {
    const router = useRouter();
    const route = useRoute();
    const store = useStore();
    const loading = ref(false);
    const loadingData = ref(true);
    
    const form = reactive({
      id: 0,
      title: '',
      priority: 2,
      status: 'Open',
      description: ''
    });

    const isPrivileged = computed(() => {
        return store.getters['auth/isStaff'] || 
               store.getters['auth/isSuperAdmin'] || 
               store.getters['auth/isTenantAdmin'];
    });

    const isReadOnly = computed(() => {
        return form.status === 'Resolved' && !isPrivileged.value;
    });

    const ticketId = Number(route.params.id);

    onMounted(async () => {
        try {
            if (!ticketId) {
                notificationService.error('Geçersiz Talep ID');
                router.back();
                return;
            }
            const data = await ticketService.getTicketDetails(ticketId);
            if (data) {
                form.id = data.id;
                form.title = data.title;
                form.description = data.description;
                if (typeof data.priority === 'string') {
                     const pMap: Record<string, number> = { 'Low': 1, 'Medium': 2, 'High': 3, 'Critical': 4 };
                     form.priority = pMap[data.priority] || 2;
                } else {
                    form.priority = data.priority;
                }
                form.status = data.status; 
            }
        } catch (error) {
            console.error('Talep detayları alınamadı', error);
            notificationService.error('Talep bulunamadı.');
            router.back();
        } finally {
            loadingData.value = false;
        }
    });

    const submitUpdate = async () => {
      if (isReadOnly.value) return;

      loading.value = true;
      try {
        const updateData = {
            id: form.id,
            title: form.title,
            description: form.description,
            priority: Number(form.priority), 
            status: form.status
        };

        // Priority int -> string mapping for API
        const pMapReverse: Record<number, string> = { 1: 'Low', 2: 'Medium', 3: 'High', 4: 'Critical' };
        const payload = {
            ...updateData,
            priority: pMapReverse[form.priority]
        };

        console.log('Updating Ticket:', payload);
        await ticketService.updateTicket(payload);
        
        notificationService.success('Talep başarıyla güncellendi.');
        router.back(); 
      } catch (error: any) {
        console.error(error);
        notificationService.error('Hata: ' + (error.message || 'Güncelleme başarısız.'));
      } finally {
        loading.value = false;
      }
    };

    return {
      form,
      loading,
      loadingData,
      submitUpdate,
      isReadOnly
    };
  }
});
</script>

<style scoped>
/* Scoped styles removed in favor of global styles */
</style>
