using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Referat
{
    public class cWords : IEnumerable<Word>
    {
        private Sentence _parentSentence;
        private List<Word> _items;
        public Sentence ParentSentence
        {
            get { return _parentSentence; }
            set { _parentSentence = value; }
        }

        // конструктор
        public cWords(Sentence parent)
        {
            this._parentSentence = parent;
            _items = new List<Word>();
        }

        // конструктор 2
        public cWords()
        {
            _items = new List<Word>();
        }

        // поиск слова по содержимому
        public int findByContent(string wordContent)
        {
            int n = _items.FindIndex(Word => Word.Content.Equals(wordContent, StringComparison.Ordinal));
            return n;
        }

        // добавление нового слова с проверкой - является ли текущее слово стоп словом
        public void Add(string text, string lexems, Sentence parent)
        {
            if (this.isStopWord(text, lexems))
            {
                return;
            }

            _items.Add(new Word(text, lexems, parent));
        }
        
        // проверка является ли текущее слово стоп-словом
        private bool isStopWord(string text, string lexem)
        {
            lexem = lexem.Replace("?", "");
            if (this._parentSentence.Parent.stopWords.hasWord(lexem))
            {
                return true;
            } 
            return false;
        }

        // добавление в колекцию
        public void Add(string text)
        {
            _items.Add(new Word(text));
        }

        // добавление в колецию - перегрузка
        public void Add(Word w)
        {
            _items.Add(w);
        }

        // Количество эл-ов в коллеции
        public int Count()
        {
            return _items.Count();
        }

        // перечисление в коллекции
        public IEnumerator<Word> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        // перечисление в коллекции
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        // удаление элемента колекции
        public void remove(int i)
        {
            _items.RemoveAt(i);
        }

        // индексатор для элементов коллекции
        public Word this[int index]
        {
            get
            {
                return _items[index];
            }

            set
            {
                _items[index] = value;
            }
        }
    }
}