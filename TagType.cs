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

        public class Tag
        {
            public string name;
            public List<Tag> innerTags;
            public Dictionary<string, string> attributes;
            public string content;

            public Tag()
            {
                innerTags = new List<Tag>();
                attributes = new Dictionary<string, string>();
            }

        }

        /// <summary>
        /// Parses a string to get the content and attributes of the first tag it finds. The tag contains any inner tags as well. 
        /// It should not containt any &gt; or &lt; sign that is not a part of a tag. Ignores any additional text before or after the said tag
        /// </summary>
        /// <param name="rawTest"> the text to be parsed </param>
        /// <returns> The tag class </returns>
        public static Tag getTagComplete(string rawTest)
        {
            string s;
            return getTagComplete(rawTest, out s, out s);
        }
        
        /// <summary>
        /// Parses a string to get the content and attributes of the first tag it finds. The tag contains any inner tags as well. 
        /// It should not containt any &gt; or &lt; sign that is not a part of a tag 
        /// </summary>
        /// <param name="rawText"> The text that contains the tag </param>
        /// <param name="contentBefore"> Any text that was before the first tag</param>
        /// <param name="contentAfter"> Any text that was after the closing tag of the first open tag </param>
        /// <returns> the Tag containing all elements </returns>
        public static Tag getTagComplete(string rawText,out string contentBefore, out string contentAfter)
        {
            int startIndex = rawText.IndexOf("<");
            if (startIndex == -1 || !rawText.Contains(">"))
            {
                contentBefore = "";
                contentAfter = "";
                return null;
            }

            contentBefore = rawText.Substring(0, startIndex);
            contentAfter = "";
            Tag tag = new Tag();

            ///flags:
            bool searchingForTagName = true, inContent = false, inQuotes = false, inContentTagOpened=false;
          //  int pos = startIndex;
            string s = "";
            string parsed = rawText.Substring(startIndex + 1);

            //foreach (char c in parsed)
            char c;
            for (int i = 0;i<parsed.Length;i++)
            {
                c = parsed[i];
                //pos++;
                if (inContentTagOpened)
                {
                    /// if there is a space right after the &lt;, it isn't considered a correct tag
                    if (c == ' ')
                    {
                        s += '<' + ' ';
                        continue;
                    }
                    ///if I'm not mistaken, this should most definately mean, that the
                    if (c == '/')
                    {
                        int ed = parsed.Substring(i - 1).IndexOf(">");
                        if (ed != -1 && parsed.Substring(i - 1, ed + 1).Equals("</" + tag.name + ">"))
                        {
                            contentAfter = parsed.Substring(i + ed);
                            tag.content += s;
                            return tag;
                        }
                        else
                        {
                            throw new Exception("Something went wrong");
                        }
                    }

                    string bef, aft;
                    tag.innerTags.Add(getTagComplete(parsed.Substring(i - 1),out bef,out aft));
                    i += (parsed.Length - i) - aft.Length;
                    continue;

                }

                if (searchingForTagName && (c == ' ' || c == '>'))
                {
                    searchingForTagName = false;
                    tag.name = s;
                    s = "";
                    continue;
                }

                if (c == '>' && !inQuotes && !inContent)
                {
                    inContent = true;
                    if (s.Length > 3 && s.Contains("="))
                    {
                        if (s.IndexOf("=") == s.Length)
                        {
                            tag.attributes.Add(s.Substring(0, s.Length - 1), "");
                        }
                        else
                        {
                            string[] sa = s.Split(new char[] { '=' }, 2);
                            tag.attributes.Add(sa[0], sa[1]);
                        }
                    }
                    s = "";
                    continue;
                }

                /// A tag that doesnt have end, but is closed in itself i.e. &lt;div /&gt;
                if (c=='/' && (!inQuotes && parsed[i+1] == '>'))
                {
                    tag.content = "";

                    if(s.Length > 3 && s.Contains("="))
                    {
                        if (s.IndexOf("=") == s.Length)
                        {
                            tag.attributes.Add(s.Substring(0, s.Length - 1), "");
                        }
                        else
                        {
                            string[] sa = s.Split(new char[] { '=' }, 2);
                            tag.attributes.Add(sa[0], sa[1]);
                        }
                    }
                    contentAfter = parsed.Substring(i + 2);
                    return tag;
                }

                if (c== '<' && inContent)
                {
                    inContentTagOpened = true;
                    continue;
                }

                // if c=='<' && !incontent it should add it

                if (inQuotes && c == '"')
                {
                    inQuotes = false;
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                    continue;
                }

                ///any other cases of c==&gt; should be handled by this point
                if (c == ' ' && !inQuotes && !inContent)
                {
                    ///the name of the tag cannot contain '='
                    if (s.Length > 3 && s.Contains("="))
                    {
                        if (s.IndexOf("=") == s.Length)
                        {
                            tag.attributes.Add(s.Substring(0, s.Length - 1), "");
                        }
                        else
                        {
                            string[] sa = s.Split(new char[] { '=' }, 2);
                            tag.attributes.Add(sa[0], sa[1]);
                        }
                    }
                    s = "";
                    continue;
                }
                s += c;
            }

            return tag;
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
