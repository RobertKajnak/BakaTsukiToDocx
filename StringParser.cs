using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class StringParser
    {
        string content;
        int start;
        int end;

        public StringParser(string target)
        {
            content = target;
        }

        /// <summary>
        /// Gets the First substring using the two strings as delimiters. Send null to set them to 0 and last character respectively
        /// </summary>
        /// <param name="from">Starting from the end of this string</param>
        /// <param name="to">Up to this string</param>
        public string Substring(string from, string to)
        {
            int tempStart = from == null ? 0 : content.IndexOf(from);
            int tempEnd = to == null ? content.Length : content.IndexOf(to);
            start = from == null ? 0 : tempStart<0 ? 0: tempStart + from.Length;
            end = to==null? content.Length : tempEnd<0? content.Length: content.IndexOf(to);
            return content.Substring(start, end - start);
        }

        /// <summary>
        /// Gets the Last substring using the two strings as delimiters. Send null to set them to 0 and last character respectively
        /// </summary>
        /// <param name="from">Starting from the end of this string</param>
        /// <param name="to">Up to this string</param>
        public string SubstringLast(string from, string to)
        {
            start = from == null ? 0 : content.IndexOf(from) + from.Length;
            end = to == null ? content.Length : content.IndexOf(to);
            if (start > end || end<0 || start < 0)
            {
                return null;
            }
            return content.Substring(start, end - start);
        }

        override
        public string ToString()
        {
            return content;
        }
    }
}
