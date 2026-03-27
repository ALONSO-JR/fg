using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using SistemaGestionCRA.Modelos;

namespace SistemaGestionCRA.Logica
{
    public class Marc21Servicio
    {
        public string ExportarAMarcXml(string rutaXml, List<Ejemplar> ejemplares)
        {
            try {
                var settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };
                using var writer = XmlWriter.Create(rutaXml, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("collection", "http://www.loc.gov/MARC21/slim");

                foreach (var e in ejemplares) {
                    writer.WriteStartElement("record");

                    // 020 - ISBN
                    if (!string.IsNullOrEmpty(e.ISBN)) {
                        writer.WriteStartElement("datafield");
                        writer.WriteAttributeString("tag", "020");
                        writer.WriteAttributeString("ind1", " ");
                        writer.WriteAttributeString("ind2", " ");
                        writer.WriteStartElement("subfield");
                        writer.WriteAttributeString("code", "a");
                        writer.WriteString(e.ISBN);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }

                    // 100 - Autor
                    if (!string.IsNullOrEmpty(e.Autor)) {
                        writer.WriteStartElement("datafield");
                        writer.WriteAttributeString("tag", "100");
                        writer.WriteAttributeString("ind1", "1");
                        writer.WriteAttributeString("ind2", " ");
                        writer.WriteStartElement("subfield");
                        writer.WriteAttributeString("code", "a");
                        writer.WriteString(e.Autor);
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }

                    // 245 - Título
                    writer.WriteStartElement("datafield");
                    writer.WriteAttributeString("tag", "245");
                    writer.WriteAttributeString("ind1", "1");
                    writer.WriteAttributeString("ind2", "0");
                    writer.WriteStartElement("subfield");
                    writer.WriteAttributeString("code", "a");
                    writer.WriteString(e.Titulo);
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    // 260 - Editorial y Año
                    writer.WriteStartElement("datafield");
                    writer.WriteAttributeString("tag", "260");
                    writer.WriteAttributeString("ind1", " ");
                    writer.WriteAttributeString("ind2", " ");
                    if (!string.IsNullOrEmpty(e.Editorial)) {
                        writer.WriteStartElement("subfield");
                        writer.WriteAttributeString("code", "b");
                        writer.WriteString(e.Editorial);
                        writer.WriteEndElement();
                    }
                    if (e.Anio > 0) {
                        writer.WriteStartElement("subfield");
                        writer.WriteAttributeString("code", "c");
                        writer.WriteString(e.Anio.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // record
                }

                writer.WriteEndElement(); // collection
                writer.WriteEndDocument();
                return "OK";
            } catch (Exception ex) {
                return ex.Message;
            }
        }
    }
}
