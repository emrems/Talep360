using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using TalepService.Wrappers;

namespace TalepService.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IdempotencyAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "X-Idempotency-Key";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var idempotencyKey))
            {
                // Header yoksa devam et (veya zorunlu kılınabilir)
                base.OnActionExecuting(context);
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Eğer user yoksa (anonymous) IP kullanılabilir, ama bu proje auth gerektiriyor.
            // Anahtar: Idempotency_{UserId}_{Key}
            var cacheKey = $"Idempotency_{userId}_{idempotencyKey}";

            if (cache.TryGetValue(cacheKey, out _))
            {
                // İşlem daha önce yapılmış veya şu an yapılıyor.
                context.Result = new ConflictObjectResult(new BaseResponse<string>("Bu işlem zaten gerçekleştirildi veya şu an işleniyor. Lütfen bekleyiniz.")
                {
                    IsSuccess = false,
                    ErrorCode = "DUPLICATE_REQUEST"
                });
                return;
            }

            // Anahtarı cache'e ekle (Örn: 1 dakika geçerli)
            // Bu basit bir kilit mekanizmasıdır. İdealde işlem bitince sonucu cachelemek gerekir.
            // Ancak "çift tıklama" önlemek için bu yeterlidir.
            cache.Set(cacheKey, true, TimeSpan.FromMinutes(1));

            base.OnActionExecuting(context);
        }
    }
}
