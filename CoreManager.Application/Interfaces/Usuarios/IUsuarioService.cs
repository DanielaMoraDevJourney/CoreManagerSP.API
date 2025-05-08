using CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios
{
    public interface IUsuarioService
    {
        /// <summary>
        /// Devuelve la lista de todos los usuarios registrados.
        /// </summary>
        Task<List<Usuario>> ObtenerTodosAsync();

        /// <summary>
        /// Devuelve un usuario específico por su ID.
        /// </summary>
        Task<Usuario?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Crea un nuevo usuario a partir del DTO recibido. Encripta la contraseña antes de guardar.
        /// </summary>
        /// <param name="dto">Objeto con los datos necesarios para crear el usuario</param>
        /// <returns>Usuario creado</returns>
        Task<Usuario> CrearAsync(UsuarioCreateDto dto);

        /// <summary>
        /// Elimina un usuario por su ID.
        /// </summary>
        /// <returns>True si fue eliminado correctamente, False si no existe</returns>
        Task<bool> EliminarAsync(int id);
    }
}
