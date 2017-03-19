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
        public enum Type {h1=0, h2, h3, div, ul, li, gt, lt, sup, span, img, a, divOpen, uncategorized};
        private static string[] tags= { "<h1>", "<h2>", "<h3>", "</div>" ,"<ul>", "<li>", "<gt>", "<lt>", "<sup", "<span", "<img", "<a" , "<div" };
        private readonly static Type[] openTags = { Type.sup, Type.span, Type.img, Type.divOpen };
        private readonly static Type[] inParagraphTags = { Type.lt, Type.gt, Type.span};

        /// <summary>
        /// Get the type of a tag.
        /// </summary>
        /// <param name="tag"> </param>
        /// <returns></returns>
        public static Type getType(string tag)
        {
            ///TODO replace this with 2 fors
            for (int i=0; i<tags.Length; i++)
            {
                if (tag.Contains(tags[i]))
                {
                    return (Type)i;
                }
            }
            
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

            int isEndLine = 0;
            while (rawText.Length > endTagIndex && rawText[endTagIndex + endTag.Length + isEndLine -1]!='\n')
            {
                isEndLine++;
            }
            cleanedText = rawText.Substring(endTagIndex + endTag.Length + isEndLine);
            return content;
        }

        /// TODO(tentative) this might actually be a lot faster if a single char array was used (i.e. the content of the string not copied n times)
        public static string[] getNestedContent(string rawText, Type expectedOuterType, out string cleanedText)
        {
            List<String> result = new List<string>();
            string outerEndTag = tags[(int)expectedOuterType].Insert(1, "/") + (openTags.Contains(expectedOuterType)?">":"");

            string processing = rawText;
            ///this should get the correct content wether the initial tag is included or not, even if there is unnecessary text before it
            if (processing.Substring(processing.IndexOf("<"), processing.IndexOf(">") + 1).Contains(tags[(int)expectedOuterType]))
            {
                processing = processing.Substring(processing.IndexOf(">") + 1);
            }
            cleanedText = processing;

            int i = 0;
            while (processing.Length != 0)
            {
                int firstBracket = processing.IndexOf('<');
                string innerTag = processing.Substring(firstBracket, processing.IndexOf('>') - firstBracket + 1);
                ///exit flag/actual exit rather
                if (innerTag.Equals(outerEndTag))
                {
                    cleanedText = cleanedText.Substring(firstBracket + outerEndTag.Length);
                    cleanedText = removeEndlinesFromBeginning(cleanedText);
                    return result.ToArray();
                }

                ///create the end tag for the tag that has been found
                string innerEndTag;
                string innerTagTruncated = innerTag;
                if (innerTag.Contains(" "))
                {
                    innerTagTruncated = innerTag.Substring(0,innerTag.IndexOf(" "));
                    innerEndTag = innerTagTruncated + ">";
                }
                else
                {
                    innerEndTag = innerTag;
                }
                innerEndTag = innerEndTag.Insert(1, "/");

                processing = processing.Substring(processing.IndexOf(innerTag) + innerTag.Length);
                
                string content = processing;

                ///check if there isn't an opened tag of the same type of the opened tag
                int endTagIndex = processing.IndexOf(innerEndTag);
                int contentEndTagIndex = endTagIndex;
                int offset = 0;
                ///this may be a lazy apporach, but it doesn't really cost speed and better than trying to rearrange the while
                if (!content.Substring(offset, contentEndTagIndex).Contains(innerTagTruncated))
                {
                    processing = processing.Substring(endTagIndex);
                }
                else {
                    while (content.Substring(offset, contentEndTagIndex).Contains(innerTagTruncated))
                    {
                        processing = processing.Substring(endTagIndex + innerEndTag.Length);
                        endTagIndex = processing.IndexOf(innerEndTag);
                        contentEndTagIndex += endTagIndex + innerEndTag.Length;
                        int offsetOld = offset;
                        offset += content.Substring(content.Substring(offset).IndexOf(innerTagTruncated) + 1).IndexOf(innerTagTruncated);
                        if (offset > contentEndTagIndex || offset == -1)
                        {
                            offset = offsetOld;
                            break;
                        }
                    }
                }
                content = content.Substring(0, contentEndTagIndex);//-innerEndTag.length
                result.Add(content);
                i++;

                processing = processing.Substring(processing.IndexOf(innerEndTag) + innerEndTag.Length);
                cleanedText = processing;
            }
      
            ///it should have exited in the while, when tag==endtag
            return null;
        }

        public static string[] getSpanContent(string rawText, Type expectedType, out string cleanedText)
        {
            return getContent(rawText, expectedType, out cleanedText, 2);
        }

        public static string[] getContent(string rawText, Type expectedType, out string cleanedText, int nestCount)
        {
            //in case there are other neted tags that have more than 2, this can be moved to parameter
            
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
            ////TODO this does not feel quite right
            while (content[endIndex + 1] == '<')
            {
                ///endindex -1 should not occur
                content = content.Substring(endIndex + 1);
                endIndex = content.IndexOf('>');

            }
            content = content.Substring(endIndex + 1);


            cleanedText = removeEndlinesFromBeginning(content);

            return result;
        }

        public static Dictionary<string,string> getTagAttributes(string tag)
        {
            int startIndex = tag.IndexOf("<");
            int endIndex = tag.IndexOf(">");

            ///TODO need to iterate over it, functions will not suffice
            if (startIndex != -1 && endIndex != -1)
            {
                Dictionary<string, string> elements = new Dictionary<string, string>();

                List<string> attribs = new List<String>();
                string s = "";
                tag = tag.Substring(tag.IndexOf("<"));
                tag = tag.Split(new char[] { ' ' }, 2)[1];
                bool inQuotes = false;
                bool ignoreSpecial = false;

                ///The order of checking characters is critical
                foreach (char c in tag)
                {
                    if (c == '>')
                    {
                        attribs.Add(s);
                        break;
                    }

                    if (c == '\\' && !ignoreSpecial)
                    {
                        ignoreSpecial = true;
                        ///TODO a testcase if the continue should be used here. Maybe including the backslash is needed
                        continue;
                    }

                    if (inQuotes && c == '"' && !ignoreSpecial)
                    {
                        inQuotes = false;
                        continue;
                    }

                    if (c == '"' && !ignoreSpecial)
                    {
                        inQuotes = true;
                        continue;
                    }
                   
                    if (c == ' ' && !inQuotes)
                    {
                        attribs.Add(s);
                        s = "";
                        continue;
                    }
                    ignoreSpecial = false;
                    s += c;
                }
                ///does not work if they are spaces between quotation marks
                //string[] attribs = tag.Substring(startIndex, endIndex - startIndex).Split(' ');

                foreach (string attrib in attribs)
                {
                    string[] sa = attrib.Split(new char[] { '=' },2);
                    elements.Add(sa[0], sa[1]);
                }
                return elements;
            }

            return null;
        }

        public static string removeEndlinesFromBeginning(string toClean)
        {
            int endlnCount = 0;
            int length = toClean.Length;
            while (endlnCount <= length && toClean[endlnCount] == '\n')
            {
                endlnCount++;
            }
            return toClean.Substring(endlnCount);
        }

        public static string removeEndlinesFromEnd(string toClean)
        {
            int endlnPos = toClean.Length - 1;
            while (endlnPos > 0 && toClean[endlnPos] == '\n')
            {
                endlnPos--;
            }
            return toClean.Substring(0,endlnPos);
        }
    }

    
}
