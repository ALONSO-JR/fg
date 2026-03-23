using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaGestionCRA.Modelos
{
    public class Institucion
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? RBD { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class Parametros
    {
        public int Id { get; set; }
        public int DiasPrestamo { get; set; }
        public int MaxLibrosPorSocio { get; set; }
        public int MaxRenovaciones { get; set; }
        public double MultaDiaria { get; set; }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string? NombreUsuario { get; set; }
        public string? PasswordHash { get; set; }
        public string? Rol { get; set; }
        public string? NombreCompleto { get; set; }
    }

    public class Socio
    {
        public int Id { get; set; }
        public string? RUT { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Curso { get; set; }
        public string? Nivel { get; set; }
        public string? Rol { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Estado { get; set; }
    }

    public class Ejemplar
    {
        public int Id { get; set; }
        public string? CodigoBarras { get; set; }
        public string? ISBN { get; set; }
        public string? Titulo { get; set; }
        public string? Autor { get; set; }
        public string? Editorial { get; set; }
        public int Anio { get; set; }
        public string? Tipo { get; set; }
        public string? ClasificacionDewey { get; set; }
        public string? Cutter { get; set; }
        public string? Ubicacion { get; set; }
        public string? Estado { get; set; }
    }

    public class Prestamo
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public int EjemplarId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string? Estado { get; set; }
        public int RenovacionesRealizadas { get; set; }

        // Propiedades de navegación para facilidad en reportes/vistas
        public string? SocioNombreCompleto { get; set; }
        public string? EjemplarTitulo { get; set; }
        public string? EjemplarCodigoBarras { get; set; }
    }

    public class Sancion
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public int? PrestamoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public double Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? Estado { get; set; }
    }

    public class Bitacora
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string? Accion { get; set; }
        public string? Detalle { get; set; }
        public string? NombreUsuario { get; set; }
    }
}
