using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Referat
{

    public class cSentences: IEnumerable<Sentence>
    {
        private List<Sentence> _items; // коллекция предложений

        private Referator _parent;     // ссылка на referator
        public Referator Parent { get { return _parent; } set { _parent = value; } }

        public cSentences(Referator parent)
        {
            _parent = parent;
            _items = new List<Sentence>();
        }


        // Добавление прелдожений
        public void Add(string text, Referator parent, int index)
        {
            // prehandle text
            _items.Add(new Sentence(text, parent, index));
        }

        /// <summary>
        /// добавление предложения как объекта sentence
        /// </summary>
        /// <param name="sentence">sentence</param>
        public void Add(Sentence sentence)
        {
            _items.Add(sentence);
        }

        // сортировка предложений
        public void Sort(Comparison<Sentence> comparison)
        {
            _items.Sort(comparison);
        }

        // взять часть предложений из коллекции
        public List<Sentence> GetRange(int startIndex, int count)
        {
            return _items.GetRange(startIndex, count);
        }

        // количество 
        public int Count()
        {
            return _items.Count();
        }

        // для перечисления
        public IEnumerator<Sentence> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        // для перечисления
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}