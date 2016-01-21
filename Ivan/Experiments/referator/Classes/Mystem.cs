using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace Referat
{
    public static class Mystem
    {
        private static ProcessStartInfo procStartInfo;

        static Mystem()
        {
            string pathToDir = Directory.GetCurrentDirectory();

            if (!File.Exists(pathToDir + "\\mystem.exe")) // если файла со стеммером нет
            {
                throw new Exception("не найден файл mystem.exe");
            }

            Mystem.procStartInfo = new ProcessStartInfo("mystem");
            Mystem.procStartInfo.RedirectStandardOutput = false; // перенаправление выходного и
            Mystem.procStartInfo.RedirectStandardInput = false;  // входного потока
            Mystem.procStartInfo.RedirectStandardError = false;
            Mystem.procStartInfo.UseShellExecute = false;
            Mystem.procStartInfo.CreateNoWindow = false;// не создавать окно CMD
            // Получение текста в виде кодировки cp1251 win
            //procStartInfo.StandardOutputEncoding = Encoding.GetEncoding(1251);
            
        }


        public static string lematizate(string inputText)
        {
            Process stemmer = new Process();
            Mystem.procStartInfo.RedirectStandardInput = true;
            Mystem.procStartInfo.RedirectStandardOutput = true;
            Mystem.procStartInfo.Arguments = "-nd -e cp866";
            stemmer.StartInfo = procStartInfo;
            stemmer.Start();
            using (StreamWriter sw = stemmer.StandardInput)
            {
                sw.WriteLine(inputText);
            }
            string resString = stemmer.StandardOutput.ReadToEnd();
            stemmer.Close();
            return resString;
        }



        public static String lematizate2XMLStr(string inputText)
        {

            
            Process stemmer = new Process();
            Mystem.procStartInfo.RedirectStandardInput = true;
            Mystem.procStartInfo.RedirectStandardOutput = true;
            Mystem.procStartInfo.Arguments = "-ed cp1251 --format xml";
            stemmer.StartInfo = procStartInfo;
            stemmer.Start();
            using (StreamWriter sw = stemmer.StandardInput)
            {
                sw.WriteLine(inputText);
            }

            string xmlResString = stemmer.StandardOutput.ReadToEnd();
            stemmer.Close();
            return xmlResString;
        }

    }
}
