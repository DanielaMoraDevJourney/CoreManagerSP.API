﻿using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras
{
    public class ResultadoMejorasAplicadasDto
    {
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public Dictionary<string, string> PerfilActualizado { get; set; }
        public SolicitudResumenDto SolicitudOriginal { get; set; }
        public SolicitudResumenDto SolicitudMejorada { get; set; }
        public List<CambioVariableDto> CambiosAplicados { get; set; } = new();

    }

}
