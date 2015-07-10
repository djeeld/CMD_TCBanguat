using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Inclusiones extras:
using System.Net;      // para HttpWebRequest, WebRequest
using System.IO;       // para Stream, StreamReader
using System.Xml.Linq; // para el parser del XML

namespace CMD_TCBanguat.Clases
{
    public class TCBanguat_Worker
    {
        public void ParseaXML(XDocument xDoc)
        {
            string Ns = "http://www.banguat.gob.gt/variables/ws/";
            /*
             * Se pueden seleccionar los nodos, su nombre y valor con esta forma y con la forma de abajo
            IEnumerable<XElement> L = xDoc.Document.Descendants(XName.Get("Var", Ns));
            foreach(XElement e in L)
            {
                Console.WriteLine(e.Element(XName.Get("fecha", Ns)).Value);
            }
             */
            try
            {
                var Datos = from r in xDoc.Document.Descendants(XName.Get("Var", Ns))
                            select new
                            {
                                Moneda = r.Element(XName.Get("moneda", Ns)).Value,
                                Fecha = r.Element(XName.Get("fecha", Ns)).Value,
                                Venta = r.Element(XName.Get("compra", Ns)).Value,
                                Compra = r.Element(XName.Get("venta", Ns)).Value,
                            };

                Console.WriteLine("Tasas de Cambio Parseadas desde XML de Banguat.gob.gt");
                Console.WriteLine("_____________________________________________________");
                Console.WriteLine(" \tMoneda\tFecha\t\tVenta\tCompra");
                foreach (var r in Datos)
                {
                    Console.WriteLine(" \t{0}\t{1}\t{2}\t{3}", r.Moneda, r.Fecha, r.Venta, r.Compra);
                }
                Console.WriteLine("_____________________________________________________");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

        }

        public XDocument ExtraeXML()
        {
            // A continuación se verán los encabezados HTTP que estaremos tratando de traer.
            //
            // POST /variables/ws/TipoCambio.asmx HTTP/1.1
            //   ==> gestionado por la configuracion del request.Method a "POST"
            // Host: www.banguat.gob.gt
            //   ==> entregado por default
            // Content-Type: text/xml; charset=utf-8
            //   ==> gestionado por la configuración del request.ContentType
            // Content-Length: length
            //   ==> gestionado por la configuración del request.ContentLength
            // SOAPAction: "http://www.banguat.gob.gt/variables/ws/TipoCambioFechaInicial"
            //   ==> gestionado por la agregación de un encabezado genérico

            // Crear la solicitud (request) y setear todos los encabezados excepto para
            // la longitud del contenido (Content Length).
            string pageName = "http://www.banguat.gob.gt/variables/ws/TipoCambio.asmx";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(pageName);
            req.Method = "POST";
            req.ContentType = "text/xml;charset=UTF-8;text/html, application/xhtml+xml, */*";
            req.Headers.Add("SOAPAction", "\"http://www.banguat.gob.gt/variables/ws/TipoCambioFechaInicial\"");

            // Los valores parámetro. En el mundo ideal la aplicación debería solicitar
            // los parámetros a través de una intefaz de usuario, para que sean variables.
            DateTime FechaFin = DateTime.Now;
            string fechaInit = new DateTime(FechaFin.Year, FechaFin.Month, 1).ToShortDateString();

            // Ahora para el XML, solo construirlo por fuerza bruta
            string xmlRequest = String.Concat(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"",
                "               xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"",
                "               xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">",
                "  <soap:Body>",
                "    <TipoCambioFechaInicial xmlns=\"http://www.banguat.gob.gt/variables/ws/\">",
                "      <fechainit>", fechaInit, "</fechainit>",
                "    </TipoCambioFechaInicial>",
                "  </soap:Body>",
                "</soap:Envelope>");


            // Someter la solicitud XML dentro de un array de bytes UTF-8 por dos razones:
            // 1. Necesitamos colocar la longitud del contenido en longitud de bytes.
            // 2. El XML será colocado dentro de una solicitud de tipo "stream", la cual
            //    gestionará los bytes, no los caractéres.
            byte[] reqBytes = new UTF8Encoding().GetBytes(xmlRequest);

            // Ahora que la solicitud (request) está codificada en un array de bytes, podemos
            // ya extraer la longitud en bytes.  Configurar los valores de los encabezados HTTP,
            // en donde el único que falta es el "content-lenght".
            req.ContentLength = reqBytes.Length;

            // Escribir el XML en la solicitud "stream".
            // Escribir el contenido de la solicitud (el XML) hacia la solicitud "strem".
            try
            {
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(reqBytes, 0, reqBytes.Length);
                }

                // En este punto, los encabezados HTTP ya están colocados y el contenido XML
                // también lo está.  Es hora entonces de llamar al servicio.
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                // Acá no vamos a parsear aún la respuesta XML.
                // En vez de eso, solo vamos a expulsar a pantalla el XML en bruto recibido satisfactoriamente.
                string xmlResponse = null;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    xmlResponse = sr.ReadToEnd();
                }
                Console.WriteLine("Banco de Guatemala informa los Tipos de Cambio del día");
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine(FechaFin.ToLongDateString());
                Console.WriteLine("---------------------------------------------------------");
                XDocument doc = XDocument.Parse(xmlResponse);
                return doc;
            }
            catch (WebException ex)
            {
                // GetRequestStream and Write puede tirar una variedad de excepciones,
                // gestionarlos está fuera del tema de este artículo.
                // En una aplicación de la vida real, deberíamos registrar un LOG File
                // (en la medida de lo posible) y gestionar dichas excepciones.
                Console.WriteLine("Error con excepción: " + ex.Message);
                Console.WriteLine("---------------------------------------------------------");
                var pageContent = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                Console.WriteLine(pageContent);
                throw;
            }
        }
    }
}
