using TalepService.Context;
using TalepService.DTOs.Attachment;
using TalepService.Entities;
using TalepService.Enums;
using TalepService.Exceptions;
using TalepService.Interfaces.Repositories;
using TalepService.Interfaces.Services;
using TalepService.Wrappers;

namespace TalepService.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ITicketRepository _ticketRepository; // Ticket var mı kontrolü için
        private readonly TalepServiceContext _context;

        public AttachmentService(IAttachmentRepository attachmentRepository, ITicketRepository ticketRepository, TalepServiceContext context)
        {
            _attachmentRepository = attachmentRepository;
            _ticketRepository = ticketRepository;
            _context = context;
        }

        public async Task<BaseResponse<AttachmentDto>> AddAttachmentAsync(CreateAttachmentDto createAttachmentDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(createAttachmentDto.TicketId);
                
                // Kural 1: Ticket silinmişse attachment eklenemez
                // Kural 4: Attachment ticket silinince görünmez (Ticket silindiyse zaten işlem yapılamaz)
                if (ticket == null) // Global Query Filter sayesinde IsDeleted olanlar zaten null gelecek
                {
                    throw new NotFoundException($"Id {createAttachmentDto.TicketId} olan talep bulunamadı.");
                }

                // Kural 2: Closed ticket’a attachment eklenmez
                if (ticket.Status == TicketStatus.Closed)
                {
                    throw new BadRequestException("Kapalı statüsündeki talebe dosya eklenemez.");
                }

                var attachment = new Attachment
                {
                    TicketId = createAttachmentDto.TicketId,
                    FilePath = createAttachmentDto.FilePath,
                    UploadBy = createAttachmentDto.UploadBy,
                    UploadedAtUtc = DateTime.UtcNow,
                    IsDeleted = false // Kural 3: Attachment soft delete olur (Entity'de default false ama açıkça belirtelim)
                };

                await _attachmentRepository.AddAsync(attachment);
                await _attachmentRepository.SaveChangesAsync();

                await transaction.CommitAsync();

                return BaseResponse<AttachmentDto>.Success(new AttachmentDto
                {
                    Id = attachment.Id,
                    TicketId = attachment.TicketId,
                    FilePath = attachment.FilePath,
                    UploadBy = attachment.UploadBy,
                    UploadedAtUtc = attachment.UploadedAtUtc
                }, "Dosya eklendi.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BaseResponse<bool>> DeleteAttachmentAsync(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                throw new NotFoundException($"Id {id} olan dosya bulunamadı.");
            }

            // Hard delete mi Soft delete mi? Entity'de IsDeleted var, soft delete yapalım.
            attachment.IsDeleted = true;
            await _attachmentRepository.UpdateAsync(attachment);
            await _attachmentRepository.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Dosya silindi.");
        }

        public async Task<BaseResponse<IEnumerable<AttachmentDto>>> GetAttachmentsByTicketIdAsync(int ticketId)
        {
            var attachments = await _attachmentRepository.GetByTicketIdAsync(ticketId);
            var dtos = attachments.Where(x => !x.IsDeleted).Select(a => new AttachmentDto
            {
                Id = a.Id,
                TicketId = a.TicketId,
                FilePath = a.FilePath,
                UploadBy = a.UploadBy,
                UploadedAtUtc = a.UploadedAtUtc
            });

            return BaseResponse<IEnumerable<AttachmentDto>>.Success(dtos);
        }
    }
}
