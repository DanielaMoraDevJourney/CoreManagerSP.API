﻿namespace CoreManagerSP.API.CoreManager.Application.DTOs.Analisis
{
    public class CriterioEvaluadoDto
    {
        public string Criterio { get; set; }
        public string ValorUsuario { get; set; }
        public string ValorRequerido { get; set; }
        public bool Cumple { get; set; }
    }
}
