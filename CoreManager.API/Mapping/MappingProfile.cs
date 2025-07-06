using AutoMapper;
using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoreManagerSP.API.CoreManager.API.Mapping
{
    /// <summary>
    /// Perfil de AutoMapper para mapear entidades a DTOs de respuesta.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EntidadFinanciera, EntidadFinancieraDto>()
                .ForMember(dest => dest.TiposPrestamo, opt =>
                    opt.MapFrom(src => src.EntidadesTipoPrestamo
                                         .Select(e => e.TipoPrestamo.Nombre)));

            // Ejemplo: otros mapeos que puedas necesitar
            // CreateMap<TipoPrestamo, TipoPrestamoComboDto>();
            // CreateMap<Usuario, UsuarioDto>();
        }
    }
}
