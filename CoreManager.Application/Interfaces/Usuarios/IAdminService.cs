using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;
using CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios
{
    public interface IAdminService
    {
        Task<List<AdminResponseDto>> ObtenerTodosAsync();
        Task<AdminResponseDto?> ObtenerPorIdAsync(int id);
        Task<AdminResponseDto> CrearAsync(AdminCreateDto dto);
        Task<bool> EliminarAsync(int id);
        Task<AdminLoginResponseDto?> LoginAsync(AdminLoginDto dto);

    }
}
