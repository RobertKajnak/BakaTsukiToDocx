using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class Sect
    {
        private string content;
        public char type;

        public Sect()
        {
            content = "";
            
        }

        public void Add(char c)
        {
            content += c;
        }

        public void Add(string s)
        {
            content += s;
        }

        public override string ToString()
        {
            return content;
        }
    }
}
