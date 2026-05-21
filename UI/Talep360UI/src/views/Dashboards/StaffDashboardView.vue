<template>
  <div class="staff-dashboard page-container">
    <header class="page-header">
      <div>
        <h2>Personel Paneli</h2>
        <p class="subtitle">Taleplerinizi buradan takip edebilirsiniz.</p>
      </div>
      <router-link to="/tickets/create" class="btn btn-primary">➕ Yeni Talep Oluştur</router-link>
    </header>

    <div class="stats-grid">
        <div class="stat-card">
            <h3>Toplam Talep</h3>
            <div class="value">{{ tickets.length }}</div>
        </div>
        <div class="stat-card">
            <h3>Açık Talepler</h3>
            <div class="value">{{ openTicketsCount }}</div>
        </div>
        <div class="stat-card">
            <h3>Çözülenler</h3>
            <div class="value">{{ resolvedTicketsCount }}</div>
        </div>
    </div>

    <div class="dashboard-tabs">
        <button 
            :class="['tab-btn', { active: activeTab === 'my' }]" 
            @click="activeTab = 'my'">
            👤 Taleplerim
        </button>
        <button 
            :class="['tab-btn', { active: activeTab === 'assigned' }]" 
            @click="activeTab = 'assigned'">
            📋 Bana Atananlar
        </button>
    </div>

    <div class="content-section">
        <div class="filters">
            <input v-model="searchQuery" type="text" placeholder="Talep ara..." class="form-control search-input" />
            <select v-model="statusFilter" class="form-control status-filter">
                <option value="">Tüm Durumlar</option>
                <option value="Open">Açık</option>
                <option value="InProgress">İşlemde</option>
                <option value="Resolved">Çözüldü</option>
                <option value="Closed">Kapandı</option>
            </select>
        </div>

        <div v-if="loading" class="loading-state">
            <div class="spinner"></div>
            Yükleniyor...
        </div>

        <div v-else-if="filteredTickets.length === 0" class="empty-state">
            Bu kategoride talep bulunamadı.
        </div>

        <div v-else class="table-container">
            <table class="table-corporate">
                <thead>
                    <tr>
                        <th style="width: 60px;">ID</th>
                        <th>Konu</th>
                        <th>Durum</th>
                        <th>Öncelik</th>
                        <th>Tarih</th>
                        <th style="width: 100px;">İşlemler</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="ticket in filteredTickets" :key="ticket.id" @click="goToDetail(ticket.id)">
                        <td class="id-cell">#{{ ticket.id }}</td>
                        <td>
                            <div class="cell-title">{{ ticket.title }}</div>
                            <div class="cell-desc">{{ truncate(ticket.description, 50) }}</div>
                            <div v-if="activeTab === 'assigned' && isPendingApproval(ticket)" class="badge badge-warning mt-2">
                                ⚠️ Onay Bekliyor
                            </div>
                        </td>
                        <td>
                            <span :class="['badge', getStatusBadgeClass(ticket.status)]">
                                {{ getStatusText(ticket.status) }}
                            </span>
                        </td>
                        <td>
                            <span :class="['priority-dot', ticket.priority.toLowerCase()]"></span>
                            {{ ticket.priority }}
                        </td>
                        <td class="date-cell">{{ formatDate(ticket.createdAtUtc) }}</td>
                        <td class="actions-cell" @click.stop>
                            <!-- Assigned Tab Actions (Staff Role) -->
                            <template v-if="activeTab === 'assigned'">
                                <template v-if="isPendingApproval(ticket)">
                                    <button class="action-btn success" @click.stop="acceptAssignment(ticket.id)" title="Görevi Kabul Et">
                                        ✅
                                    </button>
                                </template>

                                <template v-if="ticket.status === 'InProgress' || ticket.status === 'WaitingForCustomer'">
                                    <button class="action-btn success" @click.stop="resolveTicket(ticket.id)" title="Çözüldü Olarak İşaretle">
                                        🏁
                                    </button>
                                    <button class="action-btn info" @click.stop="openRequestInfoModal(ticket.id)" title="Bilgi İste">
                                        ❓
                                    </button>
                                </template>
                            </template>

                            <!-- My Tickets Tab Actions (User Role) -->
                            <template v-if="activeTab === 'my'">
                                <template v-if="ticket.status === 'WaitingForCustomer'">
                                    <button class="action-btn info" @click.stop="openReplyModal(ticket)" title="Yanıtla">
                                        ↩️
                                    </button>
                                </template>
                            </template>

                            <!-- Common Actions -->
                            <template v-if="ticket.status === 'Rejected' && ticket.rejectReason">
                                <button class="action-btn danger" @click.stop="showRejectReason(ticket)" title="Red Nedenini Gör">
                                    ℹ️
                                </button>
                            </template>
                            
                            <button v-if="ticket.status !== 'Closed' && ticket.status !== 'Rejected'" class="action-btn edit" @click.stop="editTicket(ticket.id)" title="Düzenle">
                                ✏️
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Request Info Modal -->
    <div v-if="showRequestInfoModal" class="modal-overlay" @click.self="closeRequestInfoModal">
        <div class="modal-content">
            <div class="modal-header">
                <h3>Kullanıcıdan Bilgi İste</h3>
                <button class="close-btn" @click="closeRequestInfoModal">×</button>
            </div>
            
            <div class="form-group">
                <label>İstenen Bilgi / Soru:</label>
                <textarea v-model="requestInfoDetails" class="form-control" rows="4" placeholder="Kullanıcıya sormak istediğiniz detayı yazın..."></textarea>
            </div>

            <div class="modal-actions">
                <button class="btn btn-secondary" @click="closeRequestInfoModal">İptal</button>
                <button class="btn btn-primary" @click="sendRequestInfo">Gönder</button>
            </div>
        </div>
    </div>

    <!-- Reject Reason Modal -->
    <div v-if="showRejectReasonModal" class="modal-overlay" @click.self="closeRejectReasonModal">
        <div class="modal-content">
            <div class="modal-header">
                <h3>Reddedilme Nedeni</h3>
                <button class="close-btn" @click="closeRejectReasonModal">×</button>
            </div>
            
            <div class="reject-details">
                <div class="reject-icon">❌</div>
                <div class="reject-message">
                    {{ selectedRejectReason }}
                </div>
            </div>

            <div class="modal-actions">
                <button class="btn btn-secondary" @click="closeRejectReasonModal">Kapat</button>
            </div>
        </div>
    </div>
    <!-- Reply Modal -->
    <div v-if="showReplyModal" class="modal-overlay" @click.self="closeReplyModal">
        <div class="modal-content">
            <div class="modal-header">
                <h3>Yanıt Ver</h3>
                <button class="close-btn" @click="closeReplyModal">×</button>
            </div>
            
            <div class="form-group">
                <label>Yanıtınız:</label>
                <textarea v-model="replyMessage" class="form-control" rows="4" placeholder="Yanıtınızı buraya yazınız..."></textarea>
            </div>

            <div class="modal-actions">
                <button class="btn btn-secondary" @click="closeReplyModal">İptal</button>
                <button class="btn btn-primary" @click="sendReply">Gönder</button>
            </div>
        </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, watch } from 'vue';
import ticketService, { type TicketDto } from '@/services/ticket.service';
import { useRouter } from 'vue-router';
import dialogService from '@/services/dialog.service';
import notificationService from '@/services/notification.service';

export default defineComponent({
  name: 'StaffDashboardView',
  setup() {
    const router = useRouter();
    const tickets = ref<TicketDto[]>([]);
    const loading = ref(true);
    const searchQuery = ref('');
    const statusFilter = ref('');
    const activeTab = ref('my'); // 'my' | 'assigned'

    const fetchTickets = async () => {
      try {
        loading.value = true;
        if (activeTab.value === 'my') {
            tickets.value = await ticketService.getMyTickets();
        } else {
            tickets.value = await ticketService.getAssignedTickets();
        }
      } catch (error) {
        console.error('Talepler yüklenirken hata oluştu:', error);
        tickets.value = [];
      } finally {
        loading.value = false;
      }
    };

    watch(activeTab, () => {
        fetchTickets();
    });

    const filteredTickets = computed(() => {
      return tickets.value.filter(t => {
        const matchesSearch = t.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                              t.description.toLowerCase().includes(searchQuery.value.toLowerCase());
        const matchesStatus = statusFilter.value ? t.status === statusFilter.value : true;
        return matchesSearch && matchesStatus;
      });
    });

    const openTicketsCount = computed(() => tickets.value.filter(t => t.status === 'Open').length);
    const resolvedTicketsCount = computed(() => tickets.value.filter(t => t.status === 'Resolved' || t.status === 'Closed').length);

    const formatDate = (dateStr: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('tr-TR');
    };

    const isPendingApproval = (ticket: any) => {
        return ticket.status === 'PendingApproval';
    };

    const getStatusText = (status: string) => {
        const map: Record<string, string> = {
            'Open': 'Açık',
            'Rejected': 'Reddedildi',
            'PendingApproval': 'Onay Bekliyor',
            'InProgress': 'İşlemde',
            'WaitingForCustomer': 'Bilgi Bekleniyor',
            'Resolved': 'Çözüldü',
            'Closed': 'Kapandı',
            'Canceled': 'İptal'
        };
        return map[status] || status;
    };

    const getStatusBadgeClass = (status: string) => {
        const map: Record<string, string> = {
            'Open': 'badge-warning',
            'Rejected': 'badge-danger',
            'PendingApproval': 'badge-warning',
            'InProgress': 'badge-info',
            'WaitingForCustomer': 'badge-secondary',
            'Resolved': 'badge-success',
            'Closed': 'badge-secondary',
            'Canceled': 'badge-secondary'
        };
        return map[status] || 'badge-secondary';
    };

    const truncate = (text: string, length: number) => {
        if (!text) return '';
        return text.length > length ? text.substring(0, length) + '...' : text;
    };

    const goToDetail = (id: number) => {
        // Detay sayfasına yönlendirme (henüz route tanımlı olmayabilir ama ekleyeceğiz)
        // router.push(`/tickets/${id}`);
        console.log('Go to detail', id);
    };

    const editTicket = (id: number) => {
        router.push(`/tickets/edit/${id}`);
    };

    const deleteTicket = async (id: number) => {
        const isConfirmed = await dialogService.confirm('Talebi Sil', 'Bu talebi silmek istediğinize emin misiniz?');
        if (isConfirmed) {
            try {
                await ticketService.deleteTicket(id);
                tickets.value = tickets.value.filter(t => t.id !== id);
                notificationService.success('Talep başarıyla silindi.');
            } catch (error) {
                console.error('Silme işlemi başarısız:', error);
                notificationService.error('Talep silinemedi.');
            }
        }
    };

    const acceptAssignment = async (id: number) => {
        try {
            await ticketService.acceptAssignment(id);
            const ticket = tickets.value.find(t => t.id === id);
            if (ticket) {
                ticket.status = 'InProgress';
                ticket.isAssignmentApproved = true;
            }
            notificationService.success('Görev kabul edildi, çalışma başladı.');
        } catch (error) {
            console.error('Kabul işlemi başarısız:', error);
            notificationService.error('İşlem başarısız.');
        }
    };

    const resolveTicket = async (id: number) => {
        const isConfirmed = await dialogService.confirm('Çözüldü Olarak İşaretle', 'Bu talebi çözüldü olarak işaretlemek istediğinize emin misiniz?');
        if (!isConfirmed) return;

        try {
            await ticketService.resolveTicket(id);
            const ticket = tickets.value.find(t => t.id === id);
            if (ticket) {
                ticket.status = 'Resolved';
            }
            notificationService.success('Talep çözüldü olarak işaretlendi.');
        } catch (error) {
            console.error('İşlem başarısız:', error);
            notificationService.error('İşlem başarısız.');
        }
    };

    // Request Info Logic
    const showRequestInfoModal = ref(false);
    const requestInfoDetails = ref('');
    const selectedRequestTicketId = ref<number | null>(null);

    const openRequestInfoModal = (id: number) => {
        selectedRequestTicketId.value = id;
        requestInfoDetails.value = '';
        showRequestInfoModal.value = true;
    };

    const closeRequestInfoModal = () => {
        showRequestInfoModal.value = false;
        requestInfoDetails.value = '';
        selectedRequestTicketId.value = null;
    };

    const sendRequestInfo = async () => {
        if (!selectedRequestTicketId.value || !requestInfoDetails.value) {
            notificationService.warning('Lütfen detay giriniz.');
            return;
        }

        try {
            await ticketService.requestInfo(selectedRequestTicketId.value, requestInfoDetails.value);
            const ticket = tickets.value.find(t => t.id === selectedRequestTicketId.value);
            if (ticket) {
                ticket.status = 'WaitingForCustomer';
            }
            notificationService.success('Kullanıcıdan bilgi istendi.');
            closeRequestInfoModal();
        } catch (error) {
            console.error('İşlem başarısız:', error);
            notificationService.error('İşlem başarısız.');
        }
    };

    const rejectAssignment = async (id: number) => {
        // Deprecated
    };

    const showRejectReasonModal = ref(false);
    const selectedRejectReason = ref('');

    const showRejectReason = (ticket: any) => {
        selectedRejectReason.value = ticket.rejectReason;
        showRejectReasonModal.value = true;
    };

    const closeRejectReasonModal = () => {
        showRejectReasonModal.value = false;
        selectedRejectReason.value = '';
    };

    // Reply Logic
    const showReplyModal = ref(false);
    const replyMessage = ref('');
    const selectedReplyTicketId = ref<number | null>(null);

    const openReplyModal = (ticket: any) => {
        selectedReplyTicketId.value = ticket.id;
        replyMessage.value = '';
        showReplyModal.value = true;
    };

    const closeReplyModal = () => {
        showReplyModal.value = false;
        replyMessage.value = '';
        selectedReplyTicketId.value = null;
    };

    const sendReply = async () => {
        if (!selectedReplyTicketId.value || !replyMessage.value) {
            notificationService.warning('Lütfen bir yanıt yazınız.');
            return;
        }

        try {
            await ticketService.replyToTicket(selectedReplyTicketId.value, replyMessage.value);
            const ticket = tickets.value.find(t => t.id === selectedReplyTicketId.value);
            if (ticket) {
                ticket.status = 'InProgress';
                // Description update is handled in backend, frontend doesn't need to append immediately unless we show chat history
            }
            notificationService.success('Yanıtınız iletildi.');
            closeReplyModal();
        } catch (error) {
            console.error('Yanıt gönderme başarısız:', error);
            notificationService.error('Yanıt gönderilemedi.');
        }
    };

    onMounted(() => {
        fetchTickets();
    });

    return {
        tickets,
        loading,
        searchQuery,
        statusFilter,
        activeTab,
        filteredTickets,
        openTicketsCount,
        resolvedTicketsCount,
        formatDate,
        getStatusText,
        getStatusBadgeClass,
        truncate,
        goToDetail,
        editTicket,
        deleteTicket,
        acceptAssignment,
        resolveTicket,
        openRequestInfoModal,
        closeRequestInfoModal,
        sendRequestInfo,
        showRequestInfoModal,
        requestInfoDetails,
        rejectAssignment,
        isPendingApproval,
        showRejectReason,
        showRejectReasonModal,
        selectedRejectReason,
        closeRejectReasonModal,
        // Reply
        showReplyModal,
        replyMessage,
        openReplyModal,
        closeReplyModal,
        sendReply
    };
  }
});
</script>

<style scoped>
/* Specific Reject Details Style */
.reject-details {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    padding: 1rem 0;
    margin-bottom: 1.5rem;
    background: var(--bg-color);
    border-radius: var(--border-radius);
    border: 1px solid var(--border-color);
}

.reject-icon {
    font-size: 3rem;
    margin-bottom: 1rem;
}

.reject-message {
    color: var(--text-primary);
    font-size: 1.1rem;
    line-height: 1.5;
}

</style>
