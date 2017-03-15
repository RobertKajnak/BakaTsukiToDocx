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
        public enum Type {h1=0, h2, h3, gt, lt, span, img, uncategorized};
        private static string[] tags= { "<h1>", "<h2>", "<h3>", "<gt>", "<lt>", "<span", "<img" };
        private readonly static Type[] inParagraphTags = { Type.lt, Type.gt , Type.span};

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
            else if (tag.Equals(tags[(int)Type.gt]))
                return Type.gt;
            else if (tag.Equals(tags[(int)Type.lt]))
                return Type.lt;
            else if (Regex.IsMatch(tag, tags[(int)Type.span] + "."))
                return Type.span;
            else
                return Type.uncategorized;
        }

        /// <summary>
        /// Return wether a cerain tag is inparagraph or paragraph breaker. E.g. li is a paragraph breaker, but gt is not
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool isInParagraphTag(string tag)
        {
            ///It may be faster to use a long if (tag.Equals(...)), but I really don't think this matters,
            ///when the Interop takes a couple THOUSAND times longer
            return inParagraphTags.Contains(getType(tag));
        }

        /// <summary>
        /// Return wether a cerain tag is inparagraph or paragraph breaker. E.g. li is a paragraph breaker, but gt is not
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool isInParagraphTag(Type tag)
        {
            ///It may be faster to use a long if (tag.Equals(...)), but I really don't think this matters,
            ///when the Interop takes a couple THOUSAND times longer
            return inParagraphTags.Contains(tag);
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

           /* string semiformated = rawText;
            while (semiformated[endIndex + endTag.Length] == '<' && semiformated[endIndex + endTag.Length + 1] == '/')
            {
                string temp = semiformated.Substring(endIndex);
            }*/

            int isEndLine = 0;
            while (rawText.Length > endTagIndex && rawText[endTagIndex + endTag.Length + isEndLine -1]!='\n')
            {
                isEndLine++;
            }
            cleanedText = rawText.Substring(endTagIndex + endTag.Length + isEndLine);
            return content;
        }


        public static string[] getSpanContent(string rawText, Type expectedType, out string cleanedText)
        {
            //in case there are other neted tags that have more than 2, this can be moved to parameter
            int nestCount = 2;
            string[] result = new string[nestCount];
            int endIndex = -1;
            string content =rawText;

            for (int i = 0; i < nestCount; i++)
            {
                int startIndex = content.IndexOf('>');
                if (startIndex == -1)
                {
                    cleanedText = content;
                    return null;
                }
                content = content.Substring(startIndex + 1);

                ///gets the content of the ith innermost tag
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

                endIndex = content.IndexOf('<');

                ///please note that startindex is in rawtext, while endindex is in content, i.e. endindex==length of content and startindex is irrelevant at this point
                if (endIndex == -1)
                {
                    cleanedText = rawText;
                    return null;
                }
                result[i] = content.Substring(0, endIndex);
            }

            ///if this is -1, there is a tag that is not closes
            endIndex = content.IndexOf('>');
            content = content.Substring(endIndex + 1);

            endIndex = content.IndexOf('>');
            ////TODO this does not feel quite right
            while (content[endIndex + 1] == '<')
            {

                ///endindex -1 should not occur
                content = content.Substring(endIndex + 1);
                endIndex = content.IndexOf('>');
                //int tempIndex = content.IndexOf('>');
                /*   if (tempIndex == -1)
                       break;
                   endIndex = tempIndex;*/

            }
            content = content.Substring(endIndex + 1);

            int endlnCount = 0;
            while (content[endlnCount] == '\n')
            {
                endlnCount++;
            }
            cleanedText = content.Substring(endlnCount);
            /*string endTag = tags[(int)expectedType].Replace("<", "</");
            int endTagIndex = rawText.IndexOf(endTag);*/

            /* string semiformated = rawText;
             while (semiformated[endIndex + endTag.Length] == '<' && semiformated[endIndex + endTag.Length + 1] == '/')
             {
                 string temp = semiformated.Substring(endIndex);
             }*/

            /*int isEndLine = 0;
            while (rawText.Length > endTagIndex && rawText[endTagIndex + endTag.Length + isEndLine - 1] != '\n')
            {
                isEndLine++;
            }
            cleanedText = rawText.Substring(endTagIndex + endTag.Length + isEndLine);*/
            return result;
        }

    }
}
