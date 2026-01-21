using TalepService.Context;
using TalepService.DTOs.Attachment;
using TalepService.DTOs.Ticket;
using TalepService.Entities;
using TalepService.Enums;
using TalepService.Exceptions;
using TalepService.Interfaces.Repositories;
using TalepService.Interfaces.Services;
using TalepService.Wrappers;

namespace TalepService.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly TalepServiceContext _context;

        public TicketService(ITicketRepository ticketRepository, TalepServiceContext context)
        {
            _ticketRepository = ticketRepository;
            _context = context;
        }

        public async Task<BaseResponse<TicketDto>> CreateTicketAsync(CreateTicketDto createTicketDto)
        {
            // Transaction başlatıyoruz
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ticket = new Ticket
                {
                    Title = createTicketDto.Title,
                    Description = createTicketDto.Description,
                    Priority = createTicketDto.Priority,
                    Status = TicketStatus.Open, // Varsayılan durum
                    CreatedBy = createTicketDto.CreatedBy,
                    TenantId = createTicketDto.TenantId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _ticketRepository.AddAsync(ticket);
                await _ticketRepository.SaveChangesAsync();

                // Her şey başarılıysa commit
                await transaction.CommitAsync();

                return BaseResponse<TicketDto>.Success(MapToDto(ticket), "Talep başarıyla oluşturuldu.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // ExceptionMiddleware yakalayacak
            }
        }

        public async Task<BaseResponse<TicketDto>> UpdateTicketAsync(UpdateTicketDto updateTicketDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(updateTicketDto.Id);
                if (ticket == null)
                {
                    throw new NotFoundException($"Id {updateTicketDto.Id} olan talep bulunamadı.");
                }

                // State Transition Guard
                if (updateTicketDto.Status.HasValue && ticket.Status != updateTicketDto.Status.Value)
                {
                    if (!IsValidTransition(ticket.Status, updateTicketDto.Status.Value))
                    {
                        throw new BadRequestException($"Durum geçişi geçersiz: {ticket.Status} -> {updateTicketDto.Status}");
                    }
                    ticket.Status = updateTicketDto.Status.Value;
                    
                    if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Resolved)
                    {
                        ticket.ClosedAtUtc = DateTime.UtcNow;
                    }
                }

                if (!string.IsNullOrEmpty(updateTicketDto.Title)) ticket.Title = updateTicketDto.Title;
                if (!string.IsNullOrEmpty(updateTicketDto.Description)) ticket.Description = updateTicketDto.Description;
                if (updateTicketDto.Priority.HasValue) ticket.Priority = updateTicketDto.Priority.Value;
                if (updateTicketDto.AssignedTo.HasValue) ticket.AssignedTo = updateTicketDto.AssignedTo.Value;

                await _ticketRepository.UpdateAsync(ticket);
                await _ticketRepository.SaveChangesAsync();

                await transaction.CommitAsync();

                return BaseResponse<TicketDto>.Success(MapToDto(ticket), "Talep güncellendi.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BaseResponse<bool>> DeleteTicketAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new NotFoundException($"Id {id} olan talep bulunamadı.");
            }

            ticket.IsDeleted = true; // Soft delete
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Talep silindi.");
        }

        public async Task<BaseResponse<IEnumerable<TicketDto>>> GetAllTicketsAsync()
        {
            var tickets = await _ticketRepository.FindAsync(x => !x.IsDeleted);
            var ticketDtos = tickets.Select(MapToDto).ToList();
            return BaseResponse<IEnumerable<TicketDto>>.Success(ticketDtos);
        }

        public async Task<BaseResponse<TicketDto>> GetTicketByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetTicketWithAttachmentsAsync(id);
            if (ticket == null || ticket.IsDeleted)
            {
                throw new NotFoundException($"Id {id} olan talep bulunamadı.");
            }

            return BaseResponse<TicketDto>.Success(MapToDto(ticket));
        }

        // State Transition Guard Logic
        private bool IsValidTransition(TicketStatus current, TicketStatus next)
        {
            if (current == next) return true;

            switch (current)
            {
                case TicketStatus.Open:
                    return next == TicketStatus.InProgress || next == TicketStatus.Closed; // Açık -> İşlemde veya Kapatıldı
                case TicketStatus.InProgress:
                    return next == TicketStatus.Resolved || next == TicketStatus.Closed || next == TicketStatus.Open; // İşlemde -> Çözüldü, Kapatıldı veya tekrar Açık
                case TicketStatus.Resolved:
                    return next == TicketStatus.Closed || next == TicketStatus.InProgress; // Çözüldü -> Kapatıldı veya tekrar İşlemde (Reopen)
                case TicketStatus.Closed:
                    return next == TicketStatus.InProgress; // Kapatıldı -> İşlemde (Reopen)
                default:
                    return false;
            }
        }

        public async Task<BaseResponse<List<UserWorkloadDto>>> GetWorkloadStatsAsync(List<int> userIds)
        {
            if (userIds == null || !userIds.Any())
            {
                return BaseResponse<List<UserWorkloadDto>>.Success(new List<UserWorkloadDto>(), "Kullanıcı listesi boş.");
            }

            // 1. İlgili kullanıcıların aktif (Open, InProgress) taleplerini çek
            var activeTickets = await _ticketRepository.GetActiveTicketsByUserIdsAsync(userIds);

            // 2. Grupla ve istatistik çıkar
            var workloadStats = userIds.Select(userId =>
            {
                var userTickets = activeTickets.Where(t => t.AssignedTo == userId).ToList();
                return new UserWorkloadDto
                {
                    UserId = userId,
                    ActiveTicketCount = userTickets.Count,
                    // En son atanan ticket'ın tarihi (varsa)
                    LastAssignedDateUtc = userTickets.OrderByDescending(t => t.CreatedAtUtc).FirstOrDefault()?.CreatedAtUtc
                };
            }).ToList();

            return BaseResponse<List<UserWorkloadDto>>.Success(workloadStats, "İş yükü istatistikleri hazırlandı.");
        }

        private TicketDto MapToDto(Ticket ticket)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                TenantId = ticket.TenantId,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority,
                Status = ticket.Status,
                CreatedBy = ticket.CreatedBy,
                AssignedTo = ticket.AssignedTo,
                CreatedAtUtc = ticket.CreatedAtUtc,
                ClosedAtUtc = ticket.ClosedAtUtc,
                Attachments = ticket.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    TicketId = a.TicketId,
                    FilePath = a.FilePath,
                    UploadBy = a.UploadBy,
                    UploadedAtUtc = a.UploadedAtUtc
                }).ToList()
            };
        }
    }
}
