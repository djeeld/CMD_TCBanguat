using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMD_TCBanguat
{
    public class Program
    {
        // Tema extraido de banguat.gob.gt y como leerlo en http://lakenine.com/raw-web-service-calls-in-c/
        public static void Main(string[] args)
        {
            Clases.TCBanguat_Worker Tasa = new Clases.TCBanguat_Worker();
            //Console.WriteLine(Tasa.ExtraeXML());
            Tasa.ParseaXML(Tasa.ExtraeXML());
            Console.ReadKey();
        }
    }
}
