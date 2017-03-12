using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class CharArrayParser
    {
        static List<char> breakers = new List<char>(new char[]{'<'}) ;
        char[] lineBreak = new char[] { '<','b','r',' ','/','>' };
        const char emptyLine = 'b';
        /*public static void print()
        {
            foreach (char c in breakers)
            {
                Console.WriteLine(c);
            }
        }*/
        public static List<Sect> Parse(char [] array)
        {
            List<Sect> paragraphs = new List<Sect>();
            paragraphs.Add(new Sect());
            int currentSect=0;
            char c;
            for (long i = 0; i < array.Length; i++)
            {
                /*if (breakers.Contains(array[i]))
                {*/
                c = array[i];
                string tag;
                if (breakers.Contains(c))
                {
                    /*while (array[i])
                    {

                    }*/
                }
                {
                    switch (array[i]) {
                        case emptyLine:
                            paragraphs.ElementAt(currentSect).Add('\n');
                            
                        break;
                        default:
                            paragraphs.ElementAt(currentSect).Add(c);
                            break;
                    }
                }
                    
                
               
            }

            return paragraphs;
        }
    }
}
