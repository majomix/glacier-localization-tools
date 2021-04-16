using System.Collections.Generic;
using System.Diagnostics;

namespace GlacierTextConverter.Model
{
    public class HashHolder
    {
        private readonly List<string> _english;
        private readonly List<string> _slovak;

        public HashHolder()
        {
            _english = new List<string>();
            _slovak = new List<string>();
        }

        public void Add(string english, string slovak)
        {
            _english.Add(english.Replace("\\\\n", "\\\\§n").Replace("\\n", "\r\n").Replace("\\\\§n", "\\\\n"));
            _slovak.Add(slovak.Replace("\\\\n", "\\\\§n").Replace("\\n", "\r\n").Replace("\\\\§n", "\\\\n"));
        }

        public string GetReplacement(string english)
        {
            var index = _english.LastIndexOf(english);

            if (index == -1)
            {
                //Debug.WriteLine(english);
                return english;
            }

            return _slovak[index];
        }

        public int Count()
        {
            return _slovak.Count;
        }
    }
}
