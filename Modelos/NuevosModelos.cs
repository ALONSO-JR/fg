using System;

namespace SistemaGestionCRA.Modelos
{
    public class CampoPersonalizado
    {
        public int Id { get; set; }
        public string? Entidad { get; set; } // Socio, Ejemplar
        public string? NombreCampo { get; set; }
        public string? TipoDato { get; set; } // Texto, Numero, Fecha, Lista
        public string? OpcionesLista { get; set; }
    }

    public class ValorPersonalizado
    {
        public int Id { get; set; }
        public int CampoId { get; set; }
        public int EntidadId { get; set; }
        public string? Valor { get; set; }
    }

    public class Permiso
    {
        public int Id { get; set; }
        public string? Rol { get; set; }
        public string? Modulo { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeEscribir { get; set; }
        public bool PuedeEliminar { get; set; }
    }
}
