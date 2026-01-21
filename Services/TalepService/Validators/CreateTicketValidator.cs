using FluentValidation;
using TalepService.DTOs.Ticket;

namespace TalepService.Validators
{
    public class CreateTicketValidator : AbstractValidator<CreateTicketDto>
    {
        public CreateTicketValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlık boş olamaz.")
                .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama boş olamaz.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Geçersiz öncelik değeri.");
                
            RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("Tenant Id geçerli olmalıdır.");
        }
    }
}
