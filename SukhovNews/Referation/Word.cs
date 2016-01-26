using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Referat
{
    public class Word
    {

        private Sentence _parent; // ссылка на предложение

        public Sentence Parent
        {
            get { return _parent; }
        }

        private string _content;  // текст слова
        public string Content { get { return _content; } set { _content = value; } }

        public Word normWord;    // соответствующее нормальное слово
        //public cWords noneNormWords;// коллекция ненормализованных слов
        
        /* вес слова */
        private float _weight;
        public float Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        //constructors
        public Word()
        {
            this.Content = "";
            this._weight = 0;
            //this.normWords = new cWords();
            //this.noneNormWords = new cWords();
        }
        public Word(string content)
        {
            this.Content = content;
            this._weight = 0;
            //this.normWords = new cWords();
            //this.noneNormWords = new cWords();
        }

        public Word(string content, Sentence parent)
        {
            this._parent = parent;
            this.Content = content;
            this._weight = 0;
            //this.normWords = new cWords();
            //this.noneNormWords = new cWords();
        }
        public Word(string content, int weight)
        {
            this.Content = content;
            this.Weight = weight;
            //this.normWords = new cWords();
            //this.noneNormWords = new cWords();
        }

        // основной конструктор который точно используется :)
        public Word(string content, string lexem, Sentence parent)
        {
            this._content = content;
            this._weight = 0;
            _parent = parent;
            int n = this.Parent.Parent.normalWords.findByContent(lexem);
            if (n > -1) // если нормализованное слово уже было ранее обнаружено
            {
                this.normWord = this.Parent.Parent.normalWords[n]; // то оставляем ссылку на него
            }
            else
            {
                this.normWord = new Word(lexem);                    // иначе создаем новое
                this.Parent.Parent.normalWords.Add(normWord);  // и добавляем в общую коллекцию норм. слов
            }
            this.normWord.Weight++;            
        }
    }
}