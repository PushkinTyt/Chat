using System;
using System.Text.RegularExpressions;

namespace Referat
{
    
    public class Sentence
    {
        public cWords words;         // коллекция слов

        private Referator _parent;   // ссылка на Referator
        public Referator Parent { get { return _parent; }  set {_parent = value; } }

        private int _index;          // порядковый номер предложения в тексте
        public int Index    
        {
            get { return _index; }
            set { _index = value; }
        }


        private string _originalText; // исходный текст предложения
        public string originalText { get { return _originalText; } set { _originalText = value; } }
        private float _weight;        // вес предложения
        public float Weight {
            get { return _weight; }
            set { _weight = value; }                
        }


        //конструктор
        public Sentence(string origText, Referator parent, int index)
        {
            _originalText = origText;
            _parent = parent;
            _index = index;
            words = new cWords(this);
            string stemString = Mystem.lematizate(origText);
            this.parse2WordsFromStr(stemString);
        }

        public Sentence(string origText, Referator parent, int index, int m)
        {

        }
        // парсинг предложения на слова из простой строки
        private void parse2WordsFromStr(string inpStr)
        {
            string[] wordsItems = inpStr.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);         
            Regex re = new Regex("(.+){(.+)}");
            foreach (string wItem in wordsItems)
            {
                Match match = re.Match(wItem);
                this.words.Add(match.Groups[1].Value, match.Groups[2].Value, this);
            }
            
        }


        // высчитать вес
        public void calcWeight()
        {
            float wordsWeight = 0;
            foreach (Word w in words)
            {
                wordsWeight += w.normWord.Weight;
            }

            this._weight = (wordsWeight == 0) ? 0 : wordsWeight / words.Count();
        }


    }
}