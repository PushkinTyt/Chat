using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Referat
{
    public class cStopWords: IDictionaryEnumerator<string, Word>
    {
        public cStopWords()
        {
            _items = new Dictionary<string, Word>();
        }

        private Dictionary<string, Word> _items; 
        private Referator _parent;
        public Referator Parent { get { return _parent;  } set { _parent = value; } }

        public void Add(string wordContent)
        {
            _items.Add(wordContent, new Word(wordContent));
        }

        public bool hasWord(string searchWord)
        {
            return _items.ContainsKey(searchWord);
        }


    }
}
