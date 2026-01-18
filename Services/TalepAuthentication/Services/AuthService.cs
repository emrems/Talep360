using Microsoft.AspNetCore.Identity;
using TalepAuthentication.DTOs;
using TalepAuthentication.Entities;
using TalepAuthentication.Interfaces;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<BaseResponse<TokenDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return BaseResponse<TokenDto>.Fail("Kullanıcı bulunamadı veya şifre hatalı.", "USER_NOT_FOUND");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                return BaseResponse<TokenDto>.Fail("Kullanıcı bulunamadı veya şifre hatalı.", "INVALID_CREDENTIALS");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);
            
            return BaseResponse<TokenDto>.Success(token, "Giriş başarılı.");
        }

        public async Task<BaseResponse<bool>> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BaseResponse<bool>.Fail("Bu email adresi zaten kullanımda.", "EMAIL_ALREADY_EXISTS");
            }

            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                TenantId = registerDto.TenantId,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BaseResponse<bool>.Fail($"Kayıt oluşturulurken hata oluştu: {errors}", "REGISTRATION_FAILED");
            }

            // Default role assignment (optional)
            await _userManager.AddToRoleAsync(user, "User");

            return BaseResponse<bool>.Success(true, "Kayıt başarılı.");
        }

        public async Task<BaseResponse<bool>> LogoutAsync()
        {
            
            await _signInManager.SignOutAsync();
            return BaseResponse<bool>.Success(true, "Çıkış yapıldı.");
        }
    }
}
