import api from './api';

export interface TicketDto {
    id: number;
    title: string;
    description: string;
    status: string;
    priority: string;
    // ticketType: string; // Backend'de henüz yok
    createdAtUtc: string;
    createdBy: number;
    assignedTo?: number;
    isAssignmentApproved?: boolean;
    rejectReason?: string;
    // Helper fields (Backend dönmüyor ama frontendde maplenebilir)
    creatorName?: string;
    assignedToName?: string;
}

class TicketService {
    async rejectTicket(id: number, reason: string): Promise<any> {
        return await api.post(`/api/Ticket/${id}/reject`, { reason });
    }

    async assignTicket(id: number, userId: number): Promise<any> {
        return await api.post(`/api/Ticket/${id}/assign/${userId}`);
    }

    async acceptAssignment(id: number): Promise<any> {
        return await api.post(`/api/Ticket/${id}/accept-assignment`);
    }

    /*
    async rejectAssignment(id: number): Promise<any> {
        return await api.post(`/api/Ticket/${id}/reject-assignment`);
    }
    */

    async resolveTicket(id: number): Promise<any> {
        return await api.post(`/api/Ticket/${id}/resolve`);
    }

    async closeTicket(id: number): Promise<any> {
        return await api.post(`/api/Ticket/${id}/close`);
    }

    async requestInfo(id: number, details: string): Promise<any> {
        return await api.post(`/api/Ticket/${id}/request-info`, { details });
    }

    async replyToTicket(id: number, message: string): Promise<any> {
        return await api.post(`/api/Ticket/${id}/reply`, { message });
    }

    async getMyTickets(): Promise<TicketDto[]> {
        const response = await api.get('/api/Ticket/my-tickets') as any;
        return response.data || [];
    }

    async getAssignedTickets(): Promise<TicketDto[]> {
        const response = await api.get('/api/Ticket/assigned-to-me') as any;
        return response.data || [];
    }

    async getTenantTickets(): Promise<TicketDto[]> {
        const response = await api.get('/api/Ticket/tenant-tickets') as any;
        return response.data || [];
    }

    async getWorkloadStats(userIds: number[]): Promise<any> {
        const response = await api.post('/api/ticket/workload-stats', userIds) as any;
        return response.data || [];
    }
    
    async createTicket(ticket: any, idempotencyKey?: string): Promise<any> {
        const config = idempotencyKey ? { headers: { 'X-Idempotency-Key': idempotencyKey } } : {};
        return await api.post('/api/Ticket', ticket, config);
    }

    async getTicketDetails(id: number): Promise<any> {
        const response = await api.get(`/api/Ticket/${id}`) as any;
        return response.data;
    }

    async updateTicket(ticket: any): Promise<any> {
        const response = await api.put('/api/Ticket', ticket) as any;
        return response;
    }

    async deleteTicket(id: number): Promise<any> {
        const response = await api.delete(`/api/Ticket/${id}`) as any;
        return response;
    }
}

export default new TicketService();
