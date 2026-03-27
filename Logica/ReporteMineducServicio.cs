using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;
using SistemaGestionCRA.Datos;
using SistemaGestionCRA.Modelos;
using Dapper;

namespace SistemaGestionCRA.Logica
{
    public class ReporteMineducServicio
    {
        private readonly BaseDatosHelper _db;

        public ReporteMineducServicio(BaseDatosHelper db)
        {
            _db = db;
        }

        public string GenerarReporteCirculacionPdf(string rutaPdf, DateTime desde, DateTime hasta)
        {
            try {
                using var conexion = _db.ObtenerConexion();
                conexion.Open();
                var inst = conexion.QueryFirstOrDefault<Institucion>("SELECT * FROM Institucion LIMIT 1");

                var prestamos = conexion.Query(@"
                    SELECT p.FechaPrestamo, s.Nombre, s.Apellidos, s.Rol, s.Curso, e.Titulo
                    FROM Prestamos p
                    JOIN Socios s ON p.SocioId = s.Id
                    JOIN Ejemplares e ON p.EjemplarId = e.Id
                    WHERE p.FechaPrestamo BETWEEN @Desde AND @Hasta", new { Desde = desde, Hasta = hasta }).ToList();

                using var fs = new FileStream(rutaPdf, FileMode.Create);
                var doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // Cabecera
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontSub = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                doc.Add(new Paragraph($"REPORTE DE CIRCULACIÓN - CRA", fontTitulo));
                doc.Add(new Paragraph($"Establecimiento: {inst?.Nombre} (RBD: {inst?.RBD})", fontSub));
                doc.Add(new Paragraph($"Período: {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}", fontSub));
                doc.Add(new Paragraph(" "));

                // Tabla
                var tabla = new PdfPTable(5) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 15, 25, 15, 15, 30 });

                tabla.AddCell("Fecha");
                tabla.AddCell("Socio");
                tabla.AddCell("Rol");
                tabla.AddCell("Curso");
                tabla.AddCell("Ejemplar");

                foreach (var p in prestamos) {
                    tabla.AddCell(Convert.ToDateTime(p.FechaPrestamo).ToString("dd/MM/yy"));
                    tabla.AddCell($"{p.Nombre} {p.Apellidos}");
                    tabla.AddCell(p.Rol);
                    tabla.AddCell(p.Curso);
                    tabla.AddCell(p.Titulo);
                }

                doc.Add(tabla);
                doc.Close();
                return "OK";
            } catch (Exception ex) {
                return ex.Message;
            }
        }

        public string GenerarReporteInventarioExcel(string rutaExcel)
        {
            try {
                using var conexion = _db.ObtenerConexion();
                conexion.Open();
                var ejemplares = conexion.Query("SELECT * FROM Ejemplares").ToList();

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Inventario");

                ws.Cell(1, 1).Value = "Código de Barras";
                ws.Cell(1, 2).Value = "Título";
                ws.Cell(1, 3).Value = "Autor";
                ws.Cell(1, 4).Value = "ISBN";
                ws.Cell(1, 5).Value = "Ubicación";
                ws.Cell(1, 6).Value = "Estado";

                int row = 2;
                foreach (var e in ejemplares) {
                    ws.Cell(row, 1).Value = e.CodigoBarras;
                    ws.Cell(row, 2).Value = e.Titulo;
                    ws.Cell(row, 3).Value = e.Autor;
                    ws.Cell(row, 4).Value = e.ISBN;
                    ws.Cell(row, 5).Value = e.Ubicacion;
                    ws.Cell(row, 6).Value = e.Estado;
                    row++;
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(rutaExcel);
                return "OK";
            } catch (Exception ex) {
                return ex.Message;
            }
        }
    }
}
