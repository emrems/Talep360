<template>
  <div class="manager-dashboard page-container">
    <header class="page-header">
      <div>
        <h2>Yönetici Paneli</h2>
        <p class="subtitle">Ekip taleplerini ve atamaları yönetin.</p>
      </div>
      <router-link to="/tickets/create" class="btn btn-primary">➕ Yeni Talep Oluştur</router-link>
    </header>

    <div class="dashboard-tabs">
        <button 
            :class="['tab-btn', { active: activeTab === 'assigned' }]" 
            @click="activeTab = 'assigned'">
            📋 Bana Atananlar
        </button>
        <button 
            :class="['tab-btn', { active: activeTab === 'team' }]" 
            @click="activeTab = 'team'">
            🏢 Ekip Talepleri
        </button>
        <button 
            :class="['tab-btn', { active: activeTab === 'my' }]" 
            @click="activeTab = 'my'">
            👤 Taleplerim
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
                        <th>Oluşturan</th>
                        <th>Atanan</th>
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
                        </td>
                        <td>{{ getUserName(ticket.createdBy) }}</td>
                        <td>
                            <span v-if="ticket.assignedTo || ticket.assignedToName" class="badge badge-info">
                                {{ ticket.assignedToName || getUserName(ticket.assignedTo) }}
                            </span>
                            <span v-else class="text-muted">-</span>
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
                            <button v-if="ticket.status === 'Open'" class="action-btn assign" @click.stop="openAssignModal(ticket.id)" title="Atama Yap">
                                👤
                            </button>
                            <button v-if="ticket.status === 'Open'" class="action-btn reject" @click.stop="openRejectModal(ticket.id)" title="Reddet">
                                ❌
                            </button>
                            <!-- <button class="action-btn edit" @click="editTicket(ticket.id)" title="Düzenle">
                                ✏️
                            </button> -->
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Assign Modal -->
    <div v-if="showAssignModal" class="modal-overlay" @click.self="closeAssignModal">
        <div class="modal-content">
            <h3>Personel Atama</h3>
            <p class="modal-desc">Sistem tarafından önerilen uygunluk sırasına göre personel listesi:</p>
            
            <div class="staff-list">
                <div 
                    v-for="user in suggestedStaff" 
                    :key="user.id"
                    :class="['staff-item', { selected: selectedAssignee === user.id }]"
                    @click="selectedAssignee = user.id"
                >
                    <div class="staff-info">
                        <span class="staff-name">{{ user.fullName }}</span>
                        <div class="staff-meta">
                            <span class="staff-role">{{ user.roles ? user.roles.join(', ') : (user.role || '-') }}</span>
                            <span class="workload-badge" :class="{ 'busy': user.workload > 3 }">
                                {{ user.workload }} Aktif İş
                            </span>
                        </div>
                    </div>
                    <div class="selection-indicator" v-if="selectedAssignee === user.id">✓</div>
                </div>
            </div>

            <div class="modal-actions">
                <button class="btn btn-secondary" @click="closeAssignModal">İptal</button>
                <button class="btn btn-primary" @click="assignTicket" :disabled="!selectedAssignee">
                    Atama Yap
                </button>
            </div>
        </div>

        <!-- Reject Modal -->
        <div v-if="showRejectModal" class="modal-overlay" @click="closeRejectModal">
            <div class="modal-content" @click.stop>
                <h3>Talebi Reddet</h3>
                <p>Bu talebi reddetmek istediğinize emin misiniz? Red nedeni kullanıcıya iletilecektir.</p>
                
                <div class="form-group">
                    <label>Red Nedeni (Zorunlu):</label>
                    <textarea v-model="rejectReason" class="form-control" rows="3" placeholder="Red nedenini yazınız..."></textarea>
                </div>

                <div class="modal-actions">
                    <button class="btn btn-secondary" @click="closeRejectModal">İptal</button>
                    <button class="btn btn-danger" @click="confirmRejectTicket" :disabled="!rejectReason">Reddet</button>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, watch } from 'vue';
import ticketService, { type TicketDto } from '@/services/ticket.service';
import userService from '@/services/user.service';
import { useRouter } from 'vue-router';
import dialogService from '@/services/dialog.service';
import notificationService from '@/services/notification.service';

export default defineComponent({
  name: 'ManagerDashboardView',
  setup() {
    const router = useRouter();
    const activeTab = ref<'assigned' | 'team' | 'my'>('assigned');
    const tickets = ref<TicketDto[]>([]);
    const loading = ref(false);
    const searchQuery = ref('');
    const statusFilter = ref('');
    const userMap = ref<Record<number, string>>({});
    const staffUsers = ref<any[]>([]);
    const showAssignModal = ref(false);
    const showRejectModal = ref(false);
    const rejectReason = ref('');
    const selectedTicketId = ref<number | null>(null);
    const selectedAssignee = ref<number | null>(null);

    const fetchUsers = async () => {
      try {
        const response = await userService.getTenantUsers();
        console.log('API Response:', response);
        
        // Handle both standard response structure and direct array
        let users: any[] = [];
        if ((response as any).data) {
            users = (response as any).data;
        } else if (Array.isArray(response)) {
            users = response;
        }

        console.log('Parsed Users:', users);

        if (Array.isArray(users)) {
            users.forEach((u: any) => {
                userMap.value[u.id] = u.fullName || u.FullName;
            });
            
            // Tüm kullanıcıları atama listesine ekle (Rol kısıtlaması olmadan)
            staffUsers.value = users;
            
            console.log('All Tenant Users:', staffUsers.value);
        }
      } catch (error) {
        console.error('Kullanıcı listesi alınamadı:', error);
      }
    };

    const suggestedStaff = computed(() => {
        // Calculate workload for each staff member based on current loaded tickets
        const workload: Record<number, number> = {};
        
        // Initialize with 0
        staffUsers.value.forEach(u => workload[u.id] = 0);

        // Count active tickets
        tickets.value.forEach(t => {
            if (t.assignedTo && (t.status === 'Open' || t.status === 'InProgress') && workload[t.assignedTo] !== undefined) {
                workload[t.assignedTo]++;
            }
        });

        // Return staff sorted by workload (ascending) with workload count
        return staffUsers.value.map(u => ({
            ...u,
            workload: workload[u.id] || 0
        })).sort((a, b) => a.workload - b.workload);
    });

    const getUserName = (id: number) => {
        return userMap.value[id] || id;
    };

    const fetchTickets = async () => {
      loading.value = true;
      tickets.value = []; // Clear current list
      try {
        if (activeTab.value === 'assigned') {
            tickets.value = await ticketService.getAssignedTickets();
        } else if (activeTab.value === 'team') {
            tickets.value = await ticketService.getTenantTickets();
        } else {
            tickets.value = await ticketService.getMyTickets();
        }
      } catch (error) {
        console.error('Talepler yüklenirken hata oluştu:', error);
      } finally {
        loading.value = false;
      }
    };

    // Watch tab changes to refetch data
    watch(activeTab, () => {
        fetchTickets();
    });

    const filteredTickets = computed(() => {
      return tickets.value.filter(t => {
        const creatorName = getUserName(t.createdBy).toString();
        const matchesSearch = t.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                              t.description.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                              creatorName.toLowerCase().includes(searchQuery.value.toLowerCase());
        const matchesStatus = statusFilter.value ? t.status === statusFilter.value : true;
        return matchesSearch && matchesStatus;
      });
    });

    const formatDate = (dateStr: string) => {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('tr-TR');
    };

    const getStatusText = (status: string) => {
        const map: Record<string, string> = {
            'Open': 'Açık',
            'InProgress': 'İşlemde',
            'Resolved': 'Çözüldü',
            'Closed': 'Kapandı',
            'Canceled': 'İptal'
        };
        return map[status] || status;
    };

    const getStatusBadgeClass = (status: string) => {
        const map: Record<string, string> = {
            'Open': 'badge-info',
            'InProgress': 'badge-warning',
            'Resolved': 'badge-success',
            'Closed': 'badge-secondary',
            'Canceled': 'badge-danger'
        };
        return map[status] || 'badge-secondary';
    };

    const truncate = (text: string, length: number) => {
        if (!text) return '';
        return text.length > length ? text.substring(0, length) + '...' : text;
    };

    const goToDetail = (id: number) => {
        // console.log('Go to detail', id);
        // router.push(`/tickets/${id}`);
        editTicket(id); // Detay sayfası henüz yok, edit'e yönlendir
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

    const openAssignModal = (ticketId: number) => {
        selectedTicketId.value = ticketId;
        const ticket = tickets.value.find(t => t.id === ticketId);
        selectedAssignee.value = ticket?.assignedTo || null;
        showAssignModal.value = true;
    };

    const closeAssignModal = () => {
        showAssignModal.value = false;
        selectedTicketId.value = null;
        selectedAssignee.value = null;
    };

    const assignTicket = async () => {
        if (!selectedTicketId.value || !selectedAssignee.value) {
            notificationService.warning('Lütfen bir personel seçiniz.');
            return;
        }

        try {
            const ticket = tickets.value.find(t => t.id === selectedTicketId.value);
            if (!ticket) return;

            await ticketService.assignTicket(ticket.id, selectedAssignee.value);
            
            // Update local state
            ticket.assignedTo = selectedAssignee.value;
            ticket.assignedToName = userMap.value[selectedAssignee.value];
            ticket.status = 'PendingApproval';
            ticket.isAssignmentApproved = false;
            
            notificationService.success(`Talep başarıyla ${userMap.value[selectedAssignee.value]} adlı kişiye atandı. Onay bekleniyor.`);
            
            // Filtreyi temizle ki atanan ticket listeden kaybolmasın (statüsü değiştiği için)
            if (statusFilter.value === 'Open') {
                statusFilter.value = '';
            }
            
            closeAssignModal();
        } catch (error) {
            console.error('Atama işlemi başarısız:', error);
            notificationService.error('Atama yapılamadı.');
        }
    };

    const openRejectModal = (ticketId: number) => {
        selectedTicketId.value = ticketId;
        rejectReason.value = '';
        showRejectModal.value = true;
    };

    const closeRejectModal = () => {
        showRejectModal.value = false;
        selectedTicketId.value = null;
        rejectReason.value = '';
    };

    const confirmRejectTicket = async () => {
        if (!selectedTicketId.value || !rejectReason.value) return;
        
        try {
            await ticketService.rejectTicket(selectedTicketId.value, rejectReason.value);
            
            const ticket = tickets.value.find(t => t.id === selectedTicketId.value);
            if (ticket) {
                ticket.status = 'Closed';
                ticket.rejectReason = rejectReason.value;
            }
            
            notificationService.success('Talep reddedildi.');
            closeRejectModal();
        } catch (error) {
            console.error('Red işlemi başarısız:', error);
            notificationService.error('Talep reddedilemedi.');
        }
    };

    onMounted(() => {
        fetchUsers();
        fetchTickets();
    });

    return {
        activeTab,
        tickets,
        loading,
        searchQuery,
        statusFilter,
        filteredTickets,
        formatDate,
        getStatusText,
        getStatusBadgeClass,
        truncate,
        goToDetail,
        editTicket,
        deleteTicket,
        getUserName,
        showAssignModal,
        suggestedStaff,
        selectedAssignee,
        openAssignModal,
        closeAssignModal,
        assignTicket,
        showRejectModal,
        rejectReason,
        openRejectModal,
        closeRejectModal,
        confirmRejectTicket
    };
  }
});
</script>

<style scoped>
/* Staff List Styles - Component Specific */
.staff-list {
    max-height: 300px;
    overflow-y: auto;
    border: 1px solid var(--border-color);
    border-radius: var(--border-radius);
    margin-bottom: 1.5rem;
}

.staff-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.8rem 1rem;
    border-bottom: 1px solid var(--border-color);
    cursor: pointer;
    transition: background-color 0.2s;
}

.staff-item:last-child {
    border-bottom: none;
}

.staff-item:hover {
    background-color: var(--hover-bg);
}

.staff-item.selected {
    background-color: var(--primary-light);
    border-left: 4px solid var(--primary-color);
}

.staff-info {
    display: flex;
    flex-direction: column;
}

.staff-name {
    font-weight: 500;
    color: var(--text-primary);
}

.staff-role {
    font-size: 0.8rem;
    color: var(--text-secondary);
}

.staff-meta {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-top: 0.2rem;
}

.workload-badge {
    font-size: 0.75rem;
    padding: 0.1rem 0.4rem;
    background: var(--success-bg);
    color: var(--success-color);
    border-radius: 4px;
    font-weight: 500;
}

.workload-badge.busy {
    background: var(--warning-bg);
    color: var(--warning-color);
}

.selection-indicator {
    color: var(--primary-color);
    font-weight: bold;
}
</style>
