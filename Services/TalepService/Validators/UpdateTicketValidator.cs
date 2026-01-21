using FluentValidation;
using TalepService.DTOs.Ticket;

namespace TalepService.Validators
{
    public class UpdateTicketValidator : AbstractValidator<UpdateTicketDto>
    {
        public UpdateTicketValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçersiz Ticket Id.");

            RuleFor(x => x.Title)
                .MaximumLength(200).When(x => x.Title != null)
                .WithMessage("Başlık en fazla 200 karakter olabilir.");
            
            RuleFor(x => x.Status)
                .IsInEnum().When(x => x.Status.HasValue)
                .WithMessage("Geçersiz durum değeri.");

            RuleFor(x => x.Priority)
                .IsInEnum().When(x => x.Priority.HasValue)
                .WithMessage("Geçersiz öncelik değeri.");
        }
    }
}
