using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<bool> RegistrarUsuarioAsync(RegisterRequestDto dto);

    }

}
