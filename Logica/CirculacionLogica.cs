using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Logica
{
    public class CirculacionLogica
    {
        private readonly BaseDatosHelper _db;

        public CirculacionLogica(BaseDatosHelper db)
        {
            _db = db;
        }

        public async Task<string> Prestar(int socioId, int ejemplarId)
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            // 1. Validar socio
            var socio = await conexion.QueryFirstOrDefaultAsync<Socio>("SELECT * FROM Socios WHERE Id = @Id", new { Id = socioId });
            if (socio == null) return "El socio no existe.";
            if (socio.Estado != "Activo") return $"El socio no puede realizar préstamos. Estado actual: {socio.Estado}.";

            // 2. Validar ejemplar
            var ejemplar = await conexion.QueryFirstOrDefaultAsync<Ejemplar>("SELECT * FROM Ejemplares WHERE Id = @Id", new { Id = ejemplarId });
            if (ejemplar == null) return "El ejemplar no existe.";
            if (ejemplar.Estado != "Disponible") return $"El ejemplar no está disponible. Estado: {ejemplar.Estado}.";

            // 3. Validar límites de préstamos
            var parametros = await conexion.QueryFirstOrDefaultAsync<Parametros>("SELECT * FROM Parametros LIMIT 1") ?? new Parametros { DiasPrestamo = 7, MaxLibrosPorSocio = 3 };
            int prestamosActivos = await conexion.QuerySingleAsync<int>("SELECT COUNT(1) FROM Prestamos WHERE SocioId = @SocioId AND Estado = 'Pendiente'", new { SocioId = socioId });

            if (prestamosActivos >= parametros.MaxLibrosPorSocio)
            {
                return $"El socio ya tiene el máximo permitido de préstamos ({parametros.MaxLibrosPorSocio}).";
            }

            // 4. Registrar préstamo
            using var transaccion = conexion.BeginTransaction();
            try
            {
                var fechaPrestamo = DateTime.Now;
                var fechaVencimiento = fechaPrestamo.AddDays(parametros.DiasPrestamo);

                string sqlPrestamo = @"INSERT INTO Prestamos (SocioId, EjemplarId, FechaPrestamo, FechaVencimiento, Estado)
                                       VALUES (@SocioId, @EjemplarId, @FechaPrestamo, @FechaVencimiento, 'Pendiente');";
                await conexion.ExecuteAsync(sqlPrestamo, new { SocioId = socioId, EjemplarId = ejemplarId, FechaPrestamo = fechaPrestamo, FechaVencimiento = fechaVencimiento }, transaccion);

                string sqlUpdateEjemplar = "UPDATE Ejemplares SET Estado = 'Prestado' WHERE Id = @Id;";
                await conexion.ExecuteAsync(sqlUpdateEjemplar, new { Id = ejemplarId }, transaccion);

                transaccion.Commit();
                return "OK";
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                return "Error al registrar el préstamo: " + ex.Message;
            }
        }

        public async Task<string> Devolver(int ejemplarId)
        {
            using var conexion = _db.ObtenerConexion();
            conexion.Open();

            var prestamo = await conexion.QueryFirstOrDefaultAsync<Prestamo>("SELECT * FROM Prestamos WHERE EjemplarId = @EjemplarId AND Estado = 'Pendiente'", new { EjemplarId = ejemplarId });
            if (prestamo == null) return "No se encontró un préstamo pendiente para este ejemplar.";

            using var transaccion = conexion.BeginTransaction();
            try
            {
                DateTime fechaDevolucion = DateTime.Now;

                // Actualizar préstamo
                await conexion.ExecuteAsync("UPDATE Prestamos SET FechaDevolucion = @Fecha, Estado = 'Devuelto' WHERE Id = @Id", new { Fecha = fechaDevolucion, Id = prestamo.Id }, transaccion);

                // Actualizar ejemplar
                await conexion.ExecuteAsync("UPDATE Ejemplares SET Estado = 'Disponible' WHERE Id = @Id", new { Id = ejemplarId }, transaccion);

                // Calcular sanción si aplica
                if (fechaDevolucion.Date > prestamo.FechaVencimiento.Date)
                {
                    var parametros = await conexion.QueryFirstOrDefaultAsync<Parametros>("SELECT * FROM Parametros LIMIT 1") ?? new Parametros { MultaDiaria = 0 };
                    if (parametros.MultaDiaria > 0)
                    {
                        int diasAtraso = (fechaDevolucion.Date - prestamo.FechaVencimiento.Date).Days;
                        double monto = diasAtraso * parametros.MultaDiaria;

                        await conexion.ExecuteAsync(@"INSERT INTO Sanciones (SocioId, PrestamoId, FechaInicio, Monto, Descripcion, Estado)
                                                     VALUES (@SocioId, @PrestamoId, @Fecha, @Monto, @Desc, 'Activa')",
                                                     new { SocioId = prestamo.SocioId, PrestamoId = prestamo.Id, Fecha = fechaDevolucion, Monto = monto, Desc = $"Atraso de {diasAtraso} días en ejemplar ID: {ejemplarId}" }, transaccion);
                    }
                }

                transaccion.Commit();
                return "OK";
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                return "Error al registrar la devolución: " + ex.Message;
            }
        }
    }

    public static class SeguridaLogica
    {
        public static string GenerarHashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerificarPassword(string password, string hash)
        {
            return GenerarHashPassword(password) == hash;
        }
    }
}
