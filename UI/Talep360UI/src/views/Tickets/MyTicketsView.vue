<template>
  <div class="page-container">
    <header class="page-header">
      <h2>Taleplerim</h2>
      <router-link to="/tickets/create" class="btn btn-primary">➕ Yeni Talep Oluştur</router-link>
    </header>

    <div class="filters">
      <input v-model="searchQuery" type="text" placeholder="Talep ara..." class="form-control search-input" />
      <select v-model="statusFilter" class="form-control status-filter">
        <option value="">Tüm Durumlar</option>
        <option value="Open">Açık</option>
        <option value="InProgress">İşlemde</option>
        <option value="WaitingForCustomer">Bilgi Bekleniyor</option>
        <option value="Resolved">Çözüldü</option>
        <option value="Rejected">Reddedildi</option>
        <option value="Closed">Kapandı</option>
      </select>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      Yükleniyor...
    </div>

    <div v-else-if="filteredTickets.length === 0" class="empty-state">
      <p>Görüntülenecek talep bulunamadı.</p>
    </div>

    <div v-else class="tickets-list">
      <div class="card ticket-card" v-for="ticket in filteredTickets" :key="ticket.id" :class="{ 'border-danger': ticket.status === 'Rejected', 'border-warning': ticket.status === 'WaitingForCustomer' }">
        <div class="card-header ticket-header-row">
          <div class="ticket-meta">
            <span class="ticket-id">#{{ ticket.id }}</span>
            <span class="ticket-date">{{ formatDate(ticket.createdAtUtc) }}</span>
          </div>
          <span class="badge" :class="getStatusClass(ticket.status)">
            {{ getStatusText(ticket.status) }}
          </span>
        </div>
        
        <h3 class="ticket-title">{{ ticket.title }}</h3>
        <p class="ticket-desc">{{ truncateText(ticket.description, 100) }}</p>
        
        <!-- Status Specific Alerts -->
        <div v-if="ticket.status === 'WaitingForCustomer'" class="alert alert-info">
          ℹ️ Destek ekibi sizden bilgi bekliyor.
        </div>
        <div v-if="ticket.status === 'Rejected'" class="alert alert-danger">
          ❌ Bu talep reddedildi.
        </div>

        <div class="ticket-footer">
          <span class="badge" :class="getPriorityClass(ticket.priority)">{{ ticket.priority }}</span>
          
          <div class="actions">
            <!-- Reply Button for WaitingForCustomer -->
            <button v-if="ticket.status === 'WaitingForCustomer'" 
                    @click="openReplyModal(ticket)" 
                    class="btn btn-primary btn-sm">
              ↩️ Yanıtla
            </button>

            <!-- Show Reject Reason Button -->
            <button v-if="ticket.status === 'Rejected'" 
                    @click="openRejectReasonModal(ticket)" 
                    class="btn btn-danger btn-sm">
              ❗ Red Nedeni
            </button>

            <button @click="showDetails(ticket)" class="btn btn-secondary btn-sm">Detaylar</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Reply Modal -->
    <div v-if="showReplyModal" class="modal-overlay" @click.self="closeReplyModal">
      <div class="modal-content">
        <div class="modal-header">
          <h3>Yanıt Gönder</h3>
          <button class="close-btn" @click="closeReplyModal">&times;</button>
        </div>
        <div class="modal-body">
          <p class="info-text">Destek ekibinin talebinizle ilgili sorusuna yanıt verin:</p>
          <div class="form-group">
            <textarea 
              v-model="replyMessage" 
              placeholder="Yanıtınızı buraya yazın..." 
              rows="5"
              class="form-control"
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="closeReplyModal">İptal</button>
          <button class="btn btn-primary" @click="submitReply" :disabled="!replyMessage.trim() || submitting">
            {{ submitting ? 'Gönderiliyor...' : 'Gönder' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Reject Reason Modal -->
    <div v-if="showRejectReasonModal" class="modal-overlay" @click.self="closeRejectReasonModal">
      <div class="modal-content">
        <div class="modal-header error-header">
          <h3>❌ Talep Reddedildi</h3>
          <button class="close-btn" @click="closeRejectReasonModal">&times;</button>
        </div>
        <div class="modal-body">
          <div class="reject-reason-box">
            <h4>Red Nedeni:</h4>
            <p>{{ selectedTicket?.rejectReason || 'Belirtilmemiş' }}</p>
          </div>
          <div class="ticket-summary">
            <small>Talep: #{{ selectedTicket?.id }} - {{ selectedTicket?.title }}</small>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="closeRejectReasonModal">Kapat</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, computed } from 'vue';
import ticketService, { TicketDto } from '../../services/ticket.service';

export default defineComponent({
  name: 'MyTicketsView',
  setup() {
    const tickets = ref<TicketDto[]>([]);
    const loading = ref(true);
    const searchQuery = ref('');
    const statusFilter = ref('');
    
    // Modal states
    const showReplyModal = ref(false);
    const showRejectReasonModal = ref(false);
    const selectedTicket = ref<TicketDto | null>(null);
    const replyMessage = ref('');
    const submitting = ref(false);

    const loadTickets = async () => {
      loading.value = true;
      try {
        tickets.value = await ticketService.getMyTickets();
      } catch (error) {
        console.error('Talepler yüklenirken hata:', error);
      } finally {
        loading.value = false;
      }
    };

    onMounted(() => {
      loadTickets();
    });

    const filteredTickets = computed(() => {
      return tickets.value.filter(ticket => {
        const matchesSearch = ticket.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                              ticket.description.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                              `#${ticket.id}`.includes(searchQuery.value);
        const matchesStatus = statusFilter.value === '' || ticket.status === statusFilter.value;
        return matchesSearch && matchesStatus;
      });
    });

    const formatDate = (dateStr: string) => {
      if (!dateStr) return '';
      return new Date(dateStr).toLocaleDateString('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    };

    const truncateText = (text: string, length: number) => {
      if (!text) return '';
      if (text.length <= length) return text;
      return text.substring(0, length) + '...';
    };

    const getStatusText = (status: string) => {
      const map: Record<string, string> = {
        'Open': 'Açık',
        'InProgress': 'İşlemde',
        'Resolved': 'Çözüldü',
        'Closed': 'Kapandı',
        'Rejected': 'Reddedildi',
        'PendingApproval': 'Onay Bekliyor',
        'WaitingForCustomer': 'Bilgi Bekleniyor'
      };
      return map[status] || status;
    };

    const getStatusClass = (status: string) => {
      const map: Record<string, string> = {
        'Open': 'badge-info',
        'InProgress': 'badge-info',
        'Resolved': 'badge-success',
        'Closed': 'badge-secondary',
        'Rejected': 'badge-danger',
        'PendingApproval': 'badge-warning',
        'WaitingForCustomer': 'badge-warning'
      };
      return map[status] || 'badge-secondary';
    };

    const getPriorityClass = (priority: string) => {
      const map: Record<string, string> = {
        'Low': 'badge-secondary',
        'Medium': 'badge-warning',
        'High': 'badge-danger',
        'Urgent': 'badge-danger'
      };
      return map[priority] || 'badge-secondary';
    };

    // Actions
    const openReplyModal = (ticket: TicketDto) => {
      selectedTicket.value = ticket;
      replyMessage.value = '';
      showReplyModal.value = true;
    };

    const closeReplyModal = () => {
      showReplyModal.value = false;
      selectedTicket.value = null;
    };

    const submitReply = async () => {
      if (!selectedTicket.value || !replyMessage.value.trim()) return;

      submitting.value = true;
      try {
        await ticketService.replyToTicket(selectedTicket.value.id, replyMessage.value);
        // Update local state
        selectedTicket.value.status = 'InProgress'; // Optimistic update
        const index = tickets.value.findIndex(t => t.id === selectedTicket.value?.id);
        if (index !== -1) {
          tickets.value[index].status = 'InProgress';
        }
        alert('Yanıtınız başarıyla iletildi.');
        closeReplyModal();
        loadTickets(); // Reload to be sure
      } catch (error) {
        console.error('Yanıt gönderilirken hata:', error);
        alert('Bir hata oluştu.');
      } finally {
        submitting.value = false;
      }
    };

    const openRejectReasonModal = (ticket: TicketDto) => {
      selectedTicket.value = ticket;
      showRejectReasonModal.value = true;
    };

    const closeRejectReasonModal = () => {
      showRejectReasonModal.value = false;
      selectedTicket.value = null;
    };

    const showDetails = (ticket: TicketDto) => {
      // For now just console log, or we could route to a detail page
      // But keeping it simple for now, maybe expand later
      console.log('Show details', ticket);
      alert(ticket.description);
    };

    return {
      tickets,
      loading,
      searchQuery,
      statusFilter,
      filteredTickets,
      formatDate,
      truncateText,
      getStatusText,
      getStatusClass,
      getPriorityClass,
      // Reply Modal
      showReplyModal,
      replyMessage,
      submitting,
      openReplyModal,
      closeReplyModal,
      submitReply,
      // Reject Reason Modal
      showRejectReasonModal,
      openRejectReasonModal,
      closeRejectReasonModal,
      selectedTicket,
      showDetails
    };
  }
});
</script>

<style scoped>
/* Custom layout adjustments */
.tickets-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.ticket-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.ticket-meta {
  display: flex;
  gap: 0.5rem;
  font-size: 0.85rem;
  color: var(--text-secondary);
}

.ticket-id {
  font-weight: 600;
  color: var(--primary-color);
}

.ticket-title {
  font-size: 1.1rem;
  margin-bottom: 0.5rem;
  color: var(--text-primary);
}

.ticket-desc {
  color: var(--text-secondary);
  font-size: 0.95rem;
  margin-bottom: 1rem;
}

.ticket-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid var(--border-color);
  padding-top: 0.8rem;
  margin-top: 0.8rem;
}

.actions {
  display: flex;
  gap: 0.5rem;
}

.search-input {
  flex: 1;
}

/* Custom border colors for card states */
.border-danger {
  border-left: 4px solid var(--danger-color);
}

.border-warning {
  border-left: 4px solid var(--warning-color);
}
</style>