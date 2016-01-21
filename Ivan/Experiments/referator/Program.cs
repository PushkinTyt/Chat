using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Referat
{
    class Program
    {
        static void Main(string[] args)
        {
            string inpText;
            string textEncName = "";
            using (StreamReader sr = new StreamReader("text2.txt"))
            {
                inpText = sr.ReadToEnd();
                textEncName = sr.CurrentEncoding.WebName;
            }
            Referator r = new Referator(inpText, textEncName);
            string xmlStr = r.getXml();
            Console.WriteLine(xmlStr);
            Console.ReadKey();
        }
    }


    
}
