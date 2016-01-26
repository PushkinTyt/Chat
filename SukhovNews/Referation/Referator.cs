using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using System.Text.RegularExpressions;

namespace Referat
{
    public class Referator
    {
        private cStopWords _stopWords;
        public cStopWords stopWords
        {
            get { return _stopWords; }
        }

        public cWords normalWords;         // колекция норм слов
        public cSentences Sentences;       // колекция предложений
        public readonly string textEncode; // кодировка скорее всего не нужно

        public Referator(string xml)
        {
            Sentences = new cSentences(this);


            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xml);
            XmlNodeList xSentences = xDoc.SelectNodes("se");
            foreach (XmlNode se in xSentences)
            {
                //int seIndex = int.Parse(se.Attributes.GetNamedItem("Index"));
                //Sentence sentence = new Sentence();
            }                        
        }

        // constructor
        public Referator(string inpText, string enc) 
        {
            /* загружаем словарь стоп слов */
            textEncode = enc;
            string str = "";
            string path = Directory.GetCurrentDirectory() + "\\stopWords.txt";
            if (File.Exists(path))
            {
                _stopWords = new cStopWords();
                _stopWords.Parent = this;

                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        str = sr.ReadLine();
                        _stopWords.Add(str);
                    }
                }
            }
            else
            {
                throw new Exception("не загружен список стоп слов");
            }

            /* init NormalWords list */
            this.normalWords = new cWords();

            /* делим текст на предложения */
            Regex re = new Regex(@"(?<=(?<!([А-ЯЁA-Z]\.[А-ЯЁA-Z]))[.?!]+)[\s\n\t\r]+(?=[А-ЯЁA-Z\d])");
            string[] se = re.Split(inpText);
            
            // создание предложений
            this.Sentences = new cSentences(this);
            for (int i = 0; i < se.Length; i++)
            {
                this.Sentences.Add(se[i], this, i);
            }

            // расччет весов предложений 
            foreach (Sentence s in Sentences)
            {
                s.calcWeight();
            }
            
        }
        // реферировать с заданным процентом
        public string Compress(int compressPercent)
        {

            this.Sentences.Sort((s1, s2) => s1.Weight.CompareTo(s2.Weight));
            int countOfNeedsSentences = (int)Math.Round(this.Sentences.Count() * compressPercent / 100.0);
            List<Sentence> res = this.Sentences.GetRange(this.Sentences.Count() - countOfNeedsSentences, countOfNeedsSentences);
            res.Sort((s1, s2) => s1.Index.CompareTo(s2.Index));
            string resText = "";
            foreach (Sentence se in res)
            {
                resText += se.originalText + " ";
            }
            
            return resText;
            
        }

        // вывод 5 топовых слов
        public string getTop5NormWords()
        {
            List<Word> sortedNormWords = this.normalWords.OrderByDescending(x => x.Weight).ToList();
            string s = "";
            for (int i = 0; i < 5; i++)
            {
                s += sortedNormWords[i].Content + " - " + sortedNormWords[i].Weight + "\n";
            }

            return s;
        }

        public string getXml()
        {
            XDocument xDoc = new XDocument(
                new XDeclaration ("1.0", "windows-1251", null),
                new XElement("sentences", 
                    Sentences.Select(sentence => new XElement("se", 
                        sentence.originalText,
                        new XAttribute("index", sentence.Index),
                        new XAttribute("weight", sentence.Weight)
                    ))
                ));
            //xDoc.Save("exml.xml");
            
            return xDoc.Declaration.ToString() + "\n" + xDoc.ToString();
        }

        // выводит лог всех предложений
        public void writeLogs()
        {

            //using (StreamWriter sw = new StreamWriter("normalWords.txt")) // вывод всех нормальных слов в файл
            //{
            //    foreach (Word w in this.normalWords)
            //    {
            //        sw.WriteLine("********************************");
            //        sw.WriteLine(w.Content + " | weight: " + w.Weight);
            //        sw.Write("format in text: ");
            //        foreach (Word w2 in w.noneNormWords)
            //        {
            //            sw.Write(w2.Content + " | ");
            //        }

            //        sw.WriteLine("");
            //    }
            //}

            using (StreamWriter sw = new StreamWriter("se.txt")) // вывод всех предложений и содержащихся в нем слов
            {

                sw.WriteLine(DateTime.Now.ToString());
                sw.WriteLine("count of sentences: " + this.Sentences.Count());

                foreach (Sentence se in this.Sentences)
                {
                    sw.WriteLine("");
                    sw.WriteLine("************************************");
                    sw.WriteLine("original Text: " + se.originalText);
                    sw.WriteLine("weight: " + se.Weight);
                    sw.WriteLine("index: " + se.Index);
                    foreach (Word w in se.words)
                    {
                        sw.Write(String.Format("{0}({2}):{1} |", w.Content, w.normWord.Weight, w.normWord.Content));
                    }
                }
            }
        }
    }
}