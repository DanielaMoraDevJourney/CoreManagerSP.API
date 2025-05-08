using CoreManagerSP.API.CoreManager.Application.Interfaces.Log;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly CoreManagerDbContext _context;

        public LogService(CoreManagerDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(string tipo, string mensaje, int? usuarioId = null)
        {
            var log = new LogSistema
            {
                Tipo = tipo,
                Mensaje = mensaje,
                Fecha = DateTime.UtcNow,
                UsuarioId = usuarioId
            };

            _context.LogsSistema.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
