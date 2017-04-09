using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NapiSharp.Lib;

namespace NapiSharp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("NapiSharp 1.0");

            if (args.Length == 0)
            {
                Console.WriteLine("Podaj ścieżkę do filmu");
                return 1;
            }

            var film = args[0];

            //var film = "D:\\Share\\Filmy\\Sisters.2015.UNRATED.HDRip.XviD-ETRG\\Sisters.2015.UNRATED.HDRip.XviD-ETRG.avi";
            var n = new Lib.NapiSharp();
            var f = n.DownloadSubtitle(film);
            Console.WriteLine("Napisy zapisane w " + f);
            return 0;
        }
    }
}
