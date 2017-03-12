using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    static class TagType
    {
        public enum Type {h1=0, h2, h3, uncategorized};
        private static string[] tags= { "<h1>", "<h2>", "<h3>" };

        /// <summary>
        /// Get the type of a tag.
        /// </summary>
        /// <param name="tag"> </param>
        /// <returns></returns>
        public static Type getType(string tag)
        {
            if (Regex.IsMatch(tag, tags[(int)Type.h1]))
                return Type.h1;
            else if (tag.Equals(tags[(int)Type.h2]))
                return Type.h2;
            else if (tag.Equals(tags[(int)Type.h3]))
                return Type.h3;
            else
            return Type.uncategorized;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawText"> Text including the header and other stuff </param>
        /// <param name="expectedType">the type to flush out</param>
        /// <param name="cleanedText"> the rawtext without the tag and its content </param>
        /// <returns>the content of the tag OR null on malformed tag</returns>
        public static string getContent(string rawText, Type expectedType, out string cleanedText)
        {
            int startIndex = rawText.IndexOf('>');
            if (startIndex == -1)
            {
                cleanedText = rawText;
                return null;
            }
            string content = rawText.Substring(startIndex + 1);

            ///gets the content of the first innermost tag
            while (startIndex < content.Length && content[0] == '<')
            {
                startIndex = content.IndexOf('>');
                if (startIndex == -1)
                {
                     cleanedText = rawText;
                     return null;
                }
                content = content.Substring(startIndex + 1);
            }
            int endIndex = content.IndexOf('<');
            ///please note that startindex is in rawtext, while endindex is in content, i.e. endindex==length of content and startindex is irrelevant at this point
            if (endIndex == -1)
            {
                cleanedText = rawText;
                return null;
            }
            content = content.Substring(0, endIndex);
            string endTag = tags[(int)expectedType].Replace("<", "</");
            int endTagIndex = rawText.IndexOf(endTag);
            int isEndLine = 0;
            while (rawText.Length > endTagIndex && rawText[endTagIndex + endTag.Length + isEndLine -1]!='\n')
            {
                isEndLine++;
            }
            cleanedText = rawText.Substring(endTagIndex + endTag.Length + isEndLine);
            return content;
        }
    }
}
