using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SNews
{
    public static class Logger
    {
        //private static FileStream fileStream;
        //private static StreamWriter sw;
        private static string fileName = "log.txt";

        static Logger()
        {
            //fileStream = new FileStream(fileName, FileMode.Append);
            //sw = new StreamWriter(fileStream);
            //sw.WriteLine();
            DateTime dt = DateTime.Now;
            File.AppendAllText(fileName, String.Format("\n\n\n********Log started in {0:d/M/yyyy HH:mm:ss}", dt));
            //sw.WriteLine(String.Format("********Log started in {0:d/M/yyyy HH:mm:ss}", dt));
        }

        public static void Write(string message)
        {
            File.AppendAllText(fileName, String.Format("\n{0:HH:mm:ss}: {1}", DateTime.Now, message));
           // sw.WriteLine();
        }

    }
}
