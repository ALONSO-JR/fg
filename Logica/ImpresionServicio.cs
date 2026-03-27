using System;
using System.IO;
using BarcodeStandard;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SistemaGestionCRA.Modelos;

namespace SistemaGestionCRA.Logica
{
    public class ImpresionServicio
    {
        public string GenerarCarnetSocioPdf(string rutaPdf, Socio socio, string? rbd)
        {
            try {
                using var fs = new FileStream(rutaPdf, FileMode.Create);
                // Tamaño carnet: 86x54 mm (aprox 243x153 puntos)
                var pageSize = new iTextSharp.text.Rectangle(243, 153);
                var doc = new Document(pageSize, 10, 10, 10, 10);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                var fontB = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontN = FontFactory.GetFont(FontFactory.HELVETICA, 8);

                doc.Add(new Paragraph("BIBLIOTECA CRA", fontB));
                doc.Add(new Paragraph($"Establecimiento RBD: {rbd}", fontN));
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"{socio.Nombre} {socio.Apellidos}", fontB));
                doc.Add(new Paragraph($"Curso: {socio.Curso} | Rol: {socio.Rol}", fontN));
                doc.Add(new Paragraph($"RUT: {socio.RUT}", fontN));

                // Código de barras
                var barcode = new BarcodeStandard.Barcode();
                var imgBarcode = barcode.Encode(BarcodeStandard.Type.Code128, socio.RUT ?? socio.Id.ToString(), SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White, 150, 40);

                using var ms = new MemoryStream();
                var data = imgBarcode.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                data.SaveTo(ms);
                var itextImg = iTextSharp.text.Image.GetInstance(ms.ToArray());
                itextImg.ScaleToFit(120, 30);
                doc.Add(itextImg);

                doc.Close();
                return "OK";
            } catch (Exception ex) {
                return ex.Message;
            }
        }

        public string GenerarEtiquetasPdf(string rutaPdf, Ejemplar ejemplar)
        {
            try {
                using var fs = new FileStream(rutaPdf, FileMode.Create);
                var pageSize = new iTextSharp.text.Rectangle(140, 70); // Pequeño para etiquetas
                var doc = new Document(pageSize, 5, 5, 5, 5);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                var fontS = FontFactory.GetFont(FontFactory.HELVETICA, 6);
                var fontM = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7);

                doc.Add(new Paragraph(ejemplar.Titulo?.Length > 30 ? ejemplar.Titulo.Substring(0, 27) + "..." : ejemplar.Titulo, fontM));
                doc.Add(new Paragraph($"Clasif: {ejemplar.ClasificacionDewey} {ejemplar.Cutter}", fontS));

                var barcode = new BarcodeStandard.Barcode();
                var imgBarcode = barcode.Encode(BarcodeStandard.Type.Code128, ejemplar.CodigoBarras ?? "", SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White, 120, 30);

                using var ms = new MemoryStream();
                var data = imgBarcode.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                data.SaveTo(ms);
                var itextImg = iTextSharp.text.Image.GetInstance(ms.ToArray());
                itextImg.ScaleToFit(100, 25);
                doc.Add(itextImg);

                doc.Add(new Paragraph(ejemplar.CodigoBarras, fontS));

                doc.Close();
                return "OK";
            } catch (Exception ex) {
                return ex.Message;
            }
        }
    }
}
