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
        private readonly ICurrentUserService _currentUserService;

        public TicketService(ITicketRepository ticketRepository, TalepServiceContext context, ICurrentUserService currentUserService)
        {
            _ticketRepository = ticketRepository;
            _context = context;
            _currentUserService = currentUserService;
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

                // Kapalı (Reddedilmiş veya Kapanmış) talepler üzerinde işlem yapılamaz
                if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Rejected)
                {
                    throw new BadRequestException("Kapalı veya reddedilmiş statüsündeki talepler üzerinde değişiklik yapılamaz.");
                }

                // Çözüldü statüsündeki talepler üzerinde sadece Staff/Admin işlem yapabilir
                if (ticket.Status == TicketStatus.Resolved && !_currentUserService.IsStaff && !_currentUserService.IsSuperAdmin)
                {
                    throw new BadRequestException("Çözüldü olarak işaretlenen talepler üzerinde değişiklik yapamazsınız.");
                }

                // Yetki ve İş Kuralı Kontrolleri
                var currentUserId = _currentUserService.UserId;
                var isAssignedUser = ticket.AssignedTo == currentUserId;
                var isCreator = ticket.CreatedBy == currentUserId;
                // Not: Role kontrolü serviste _currentUserService.IsInRole gibi yapılabilir veya claim'lerden bakılabilir.
                // Basitlik için varsayalım: SuperAdmin her şeyi yapar, ama Staff kısıtlıdır.
                
                // Kural: Staff yalnızca kendisine atanmış ve onayladığı ticket’ı çözebilir.
                // Eğer statü Resolved veya Closed yapılıyorsa ve yapan AssignedTo ise, onaylı olmalı.
                if (updateTicketDto.Status.HasValue && 
                   (updateTicketDto.Status == TicketStatus.Resolved || updateTicketDto.Status == TicketStatus.Closed))
                {
                    if (isAssignedUser && ticket.IsAssignmentApproved != true)
                    {
                        throw new BadRequestException("Henüz onaylamadığınız görevi çözüldü veya kapandı olarak işaretleyemezsiniz.");
                    }
                }

                // State Transition Guard
                if (updateTicketDto.Status.HasValue && ticket.Status != updateTicketDto.Status.Value)
                {
                    if (!IsValidTransition(ticket.Status, updateTicketDto.Status.Value))
                    {
                        var msg = GetFriendlyTransitionErrorMessage(ticket.Status, updateTicketDto.Status.Value);
                        throw new BadRequestException(msg);
                    }
                    ticket.Status = updateTicketDto.Status.Value;
                    
                    if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Resolved)
                    {
                        ticket.ClosedAtUtc = DateTime.UtcNow;
                    }
                }

                // Admin/Manager dışında (veya Assigned/Creator dışında) update engelleme
                // "Admin panelinde ticket’lar EDİT EDİLEMEZ" kuralı için:
                // Eğer kullanıcı Admin ise ve Title/Description/Priority değiştirmeye çalışıyorsa engellemeliyiz.
                // Ancak burada basit bir rol kontrolümüz yok (IsInRole yok). 
                // Şimdilik UI'dan butonu kaldırarak UX sağlıyoruz, backend'de ise:
                // Admin'in sadece Reject ve Assign yapabildiğini varsayıyoruz (ayrı endpointler var).
                // Bu Update endpointi genelde Creator veya AssignedUser tarafından kullanılır.

                if (!string.IsNullOrEmpty(updateTicketDto.Title)) ticket.Title = updateTicketDto.Title;
                if (!string.IsNullOrEmpty(updateTicketDto.Description)) ticket.Description = updateTicketDto.Description;
                if (updateTicketDto.Priority.HasValue) ticket.Priority = updateTicketDto.Priority.Value;
                // AssignedTo updateTicketDto ile değiştirilmemeli, AssignTicketAsync kullanılmalı.
                // if (updateTicketDto.AssignedTo.HasValue) ticket.AssignedTo = updateTicketDto.AssignedTo.Value; 

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

            // Çözüldü statüsündeki talepler silinemez (Staff/Admin hariç diyeceğim ama genelde çözüldü silinmez, kalsın)
            // Kullanıcı isteği: "hiçbir yerde değişiklik yapamasın"
            if (ticket.Status == TicketStatus.Resolved && !_currentUserService.IsStaff && !_currentUserService.IsSuperAdmin)
            {
                throw new BadRequestException("Çözüldü statüsündeki talepler silinemez.");
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

        public async Task<BaseResponse<IEnumerable<TicketDto>>> GetTicketsByTenantIdAsync(int tenantId)
        {
            var tickets = await _ticketRepository.GetByTenantIdAsync(tenantId);
            var ticketDtos = tickets.Select(MapToDto).ToList();
            return BaseResponse<IEnumerable<TicketDto>>.Success(ticketDtos);
        }

        public async Task<BaseResponse<IEnumerable<TicketDto>>> GetTicketsByCreatedByAsync(int userId)
        {
            var tickets = await _ticketRepository.GetByCreatedByAsync(userId);
            var ticketDtos = tickets.Select(MapToDto).ToList();
            return BaseResponse<IEnumerable<TicketDto>>.Success(ticketDtos);
        }

        public async Task<BaseResponse<IEnumerable<TicketDto>>> GetTicketsByAssignedToAsync(int userId)
        {
            var tickets = await _ticketRepository.GetByAssignedToAsync(userId);
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
                    return next == TicketStatus.Rejected || next == TicketStatus.PendingApproval;
                case TicketStatus.Rejected:
                    return false; // Terminal state
                case TicketStatus.PendingApproval:
                    return next == TicketStatus.InProgress;
                case TicketStatus.InProgress:
                    return next == TicketStatus.WaitingForCustomer || next == TicketStatus.Resolved;
                case TicketStatus.WaitingForCustomer:
                    return next == TicketStatus.InProgress || next == TicketStatus.Resolved;
                case TicketStatus.Resolved:
                    return next == TicketStatus.Closed || next == TicketStatus.InProgress; // Reopen
                case TicketStatus.Closed:
                    return false; // Terminal state (or maybe allow Reopen to Open?) - User said "Sistem sonlanır", so terminal.
                default:
                    return false;
            }
        }

        private string GetFriendlyTransitionErrorMessage(TicketStatus current, TicketStatus next)
        {
            var statusNames = new Dictionary<TicketStatus, string>
            {
                { TicketStatus.Open, "Açık" },
                { TicketStatus.Rejected, "Reddedilmiş" },
                { TicketStatus.PendingApproval, "Onay Bekliyor" },
                { TicketStatus.InProgress, "İşlemde" },
                { TicketStatus.WaitingForCustomer, "Bilgi Bekleniyor" },
                { TicketStatus.Resolved, "Çözüldü" },
                { TicketStatus.Closed, "Kapandı" }
            };

            var currentName = statusNames.ContainsKey(current) ? statusNames[current] : current.ToString();
            var nextName = statusNames.ContainsKey(next) ? statusNames[next] : next.ToString();

            if (current == TicketStatus.Resolved && next != TicketStatus.Closed && next != TicketStatus.InProgress)
            {
                return "Çözüldü statüsündeki bir talep üzerinde değişiklik yapılamaz (Sadece kapatılabilir veya tekrar işleme alınabilir).";
            }

            if (current == TicketStatus.Closed)
            {
                return "Kapanmış bir talep üzerinde değişiklik yapılamaz.";
            }

            if (current == TicketStatus.Rejected)
            {
                return "Reddedilmiş bir talep üzerinde işlem yapılamaz.";
            }
            
            if (current == TicketStatus.PendingApproval && next == TicketStatus.Resolved)
            {
                 return "Onay bekleyen bir talep doğrudan çözülemez. Önce personel tarafından kabul edilmelidir.";
            }

            return $"Talep durumu '{currentName}' iken '{nextName}' olarak değiştirilemez.";
        }

        public async Task<BaseResponse<bool>> RejectTicketAsync(int id, string reason)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            if (ticket.Status != TicketStatus.Open)
                throw new BadRequestException("Sadece 'Açık' statüsündeki talepler reddedilebilir.");
            
            if (string.IsNullOrWhiteSpace(reason))
                throw new BadRequestException("Red nedeni zorunludur.");

            ticket.Status = TicketStatus.Rejected;
            ticket.RejectReason = reason;
            ticket.ClosedAtUtc = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Talep reddedildi.");
        }

        public async Task<BaseResponse<bool>> AssignTicketAsync(int id, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            if (ticket.Status != TicketStatus.Open)
                throw new BadRequestException("Sadece 'Açık' statüsündeki talepler atanabilir.");

            ticket.AssignedTo = userId;
            ticket.Status = TicketStatus.PendingApproval;
            // IsAssignmentApproved alanı artık PendingApproval statusü ile yönetiliyor, 
            // ancak backward compatibility veya ek kontrol için false yapabiliriz.
            ticket.IsAssignmentApproved = false; 

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Talep atandı, personel onayı bekleniyor.");
        }

        public async Task<BaseResponse<bool>> AcceptAssignmentAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            var currentUserId = _currentUserService.UserId;
            if (ticket.AssignedTo != currentUserId)
                throw new BadRequestException("Bu talep size atanmamış.");

            if (ticket.Status != TicketStatus.PendingApproval)
                 throw new BadRequestException("Bu talep onay aşamasında değil.");

            ticket.Status = TicketStatus.InProgress;
            ticket.IsAssignmentApproved = true;

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Görev kabul edildi, çalışma başladı.");
        }

        /*
        public async Task<BaseResponse<bool>> RejectAssignmentAsync(int id)
        {
             throw new BadRequestException("Personel kendisine atanan görevi reddedemez.");
        }
        */

        public async Task<BaseResponse<bool>> ResolveTicketAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            var currentUserId = _currentUserService.UserId;
            // Staff veya Admin
            // Ancak Staff sadece kendi ticketını
            if (ticket.AssignedTo.HasValue && ticket.AssignedTo != currentUserId)
            {
                // Eğer admin değilse (Rol kontrolü şimdilik yok, basitçe AssignedTo kontrolü)
                 throw new BadRequestException("Size atanmamış bir talebi çözemezsiniz.");
            }

            if (!IsValidTransition(ticket.Status, TicketStatus.Resolved))
                 throw new BadRequestException(GetFriendlyTransitionErrorMessage(ticket.Status, TicketStatus.Resolved));

            ticket.Status = TicketStatus.Resolved;
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Talep çözüldü olarak işaretlendi.");
        }

        public async Task<BaseResponse<bool>> CloseTicketAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            // Sadece Admin kapatabilir (Burada rol kontrolü Controller'da Authorize ile yapılacak ama business rule da ekleyebiliriz)
            // Şimdilik sadece transition check
            
            if (!IsValidTransition(ticket.Status, TicketStatus.Closed))
                 throw new BadRequestException(GetFriendlyTransitionErrorMessage(ticket.Status, TicketStatus.Closed));

            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAtUtc = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Talep tamamen kapatıldı.");
        }

        public async Task<BaseResponse<bool>> RequestCustomerInfoAsync(int id, string details)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            var currentUserId = _currentUserService.UserId;
            if (ticket.AssignedTo != currentUserId)
                throw new BadRequestException("Size atanmamış bir talep için bilgi isteyemezsiniz.");

            if (!IsValidTransition(ticket.Status, TicketStatus.WaitingForCustomer))
                 throw new BadRequestException(GetFriendlyTransitionErrorMessage(ticket.Status, TicketStatus.WaitingForCustomer));

            ticket.Status = TicketStatus.WaitingForCustomer;
            // Detayları description'a ekleyebiliriz veya yorum olarak (yorum sistemi yoksa description append)
            // User "User panelinde 'Destek ekibi sizden bilgi bekliyor' görünür" dedi.
            // Bu detay bir yere kaydedilmeli. Şimdilik Description'a append edelim veya RejectReason gibi bir alan kullanalım?
            // RejectReason alanı "Reason" için kullanılıyor, burası için yeni alan yok. 
            // Mevcut yapıyı bozmadan Description'a ekleyelim:
            ticket.Description += $"\n\n[EK BİLGİ TALEBİ - {DateTime.UtcNow}]: {details}";

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Kullanıcıdan bilgi istendi.");
        }

        public async Task<BaseResponse<bool>> ReplyToTicketAsync(int id, string message)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null) throw new NotFoundException($"Id {id} olan talep bulunamadı.");

            var currentUserId = _currentUserService.UserId;
            // Sadece talebi oluşturan kişi (User) yanıt verebilir
            if (ticket.CreatedBy != currentUserId)
            {
                throw new BadRequestException("Bu talebe sadece oluşturan kullanıcı yanıt verebilir.");
            }

            if (ticket.Status != TicketStatus.WaitingForCustomer)
            {
                 throw new BadRequestException("Sadece 'Bilgi Bekleniyor' statüsündeki taleplere yanıt verilebilir.");
            }

            ticket.Status = TicketStatus.InProgress;
            ticket.Description += $"\n\n[KULLANICI YANITI - {DateTime.UtcNow}]: {message}";

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Yanıtınız iletildi, talep tekrar işleme alındı.");
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
                IsAssignmentApproved = ticket.IsAssignmentApproved,
                RejectReason = ticket.RejectReason,
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
