using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace Baka_Tsuki_Downloader
{
    class Downloader
    {
        static string path = Directory.GetCurrentDirectory() + "\\data\\";

        private static string[] lineSeparators = { "<p>", "\n</p>" };
        private static string[] chapterSeparator = { "<h2>" };
        string _author;
        public void setAuthor(string author)
        {
            _author = author;
        }

        WebClient webClient;

        bool isLimitedTest = false;
        string tempString;

        public Downloader()
        {
            webClient = new System.Net.WebClient();
            webClient.Encoding = Encoding.UTF8;
            _author = "";
        }

        public Downloader(bool setLimitedTest)
        {
            isLimitedTest = setLimitedTest;
        }

        public void DownloadAndConvert(string URL)
        {
            string html = webClient.DownloadString(URL);
            Convert(html,null);
        }

        public void DownloadToHTML(string URL, string fileName)
        {
            //WordWriter wordWriter = new WordWriter(fileName);

            string html = webClient.DownloadString(URL);

            System.IO.File.WriteAllText(fileName, html);

        }

        public string ReadHTML(string sourceFile)
        {
            Logger.Start("Read & Convert");
            Console.WriteLine("Reading soulrce file");
            string html = File.ReadAllText(path + sourceFile);
            return html;
        }

        public void Convert(string html)
        {
            this.Convert(html, null);
        }


        /// <summary>
        /// TODO - does not work;
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string FetchAuthor(string url)
        {
            string html;
            try {
                  html = webClient.DownloadString(url);
            }
            catch
            {
            try {
                    html = File.ReadAllText(path + url);
                }
                catch
                {
                    return "Unknown";
                }
                
            }

            
            string result;
            int startIndex = html.IndexOf("written by "); 
            if (startIndex != -1)
            {
                /// 11 == length of "written by
                result = html.Substring(startIndex + 11);
                ///this can be done way more efficiently, with s[i]
                int pointIndex = result.IndexOf(".");
                int commaIndex = result.IndexOf(",");
                int colonIndex = result.IndexOf(";");
                if (pointIndex == -1)
                    pointIndex = int.MaxValue;
                if (commaIndex == -1)
                    commaIndex = int.MaxValue;
                if (colonIndex == -1)
                    colonIndex = int.MaxValue;

                int finalIndex = pointIndex < commaIndex ? pointIndex : commaIndex;
                if (colonIndex < finalIndex)
                    finalIndex = colonIndex;

                if (finalIndex < 50)
                    return result.Substring(0, finalIndex);
                

            }
            return "Unkown";
        }

        public void Convert(string html, string destFile)
        {
            Console.WriteLine("Creating Parser");
            StringParser content = new StringParser(html);
            StringParser titleHtml = new StringParser(content.Substring("<title>", "</title"));

            string title = titleHtml.SubstringLast(null, ":");
            int volume;
            Int32.TryParse(titleHtml.SubstringLast(":Volume", " - Baka-Tsuki").ToString(), out volume);

            if (destFile == null || destFile == "")
            {
                destFile = title + " - Volume " + volume + ".docx";
            }

            ///TOD One could try going back to the collection page and search for the phrase "Written by"
            Console.WriteLine("Creating WordBuilder");
            WordWriter wordWriter = new WordWriter(destFile);
            string author = _author ;
            /*if (isLimitedTest)
            {
                author = FetchAuthor("Ultimate Antihero - Baka-Tsuki.html");
            }
            else {
                author = FetchAuthor("https://www.baka-tsuki.org/project/index.php?title=" + title.Replace(' ', '_'));
            }*/
            wordWriter.Title(title, author, volume);


            //TOOD:Handle <Pre> ???
            content = new StringParser(content.Substring("<h2>", "<td> Back to <a"));

            Console.WriteLine("Starting Conversion");
            string[] chapters = content.ToString().Split(chapterSeparator, StringSplitOptions.RemoveEmptyEntries);
            Tag footNotes = TagType.getTagComplete(chapters.Last().Substring(chapters.Last().IndexOf("</h2>")+5));

            chapters = chapters.Take(chapters.Length - 1).ToArray();
            Logger.Stop("Read & Convert");
            Logger.Start("Write");
            int i = 0;
            foreach (string chapter in chapters)
            {
                if (isLimitedTest && i >= 3 )
                    break;
                if (i == 0)
                {
                    i++;
                    continue;
                }
                
                //inserting everything at once with \n instead of paragraph by paragraph increased speed by ~1000 times
                ///TODO change br similar to div
                string chapterContent = chapter.Replace("<p>", "").Replace("</p>","").Replace("<br />","\n").Replace("&lt;","<lt>").Replace("&gt;","<gt>");
                //wordWriter.Paragraph(chapter);


                string chapterTitle = TagType.getContent(chapterContent, TagType.Type.h2, out chapterContent);
                wordWriter.Chapter(chapterTitle);

                
                //TODO handle '&' for italic etc. tags 

                string buffer = "";
                float spaceBefore = -1, spaceAfter = -1;
                while (chapterContent.Length != 0)
                {
                    Tag tagAndContent = null;

                    int firstIndex = chapterContent.IndexOf('<');

                    int closeBracketIndex = chapterContent.IndexOf('>');
                    closeBracketIndex = closeBracketIndex != -1 ? closeBracketIndex : chapterContent.Length;
                    string tag=  "";
                    TagType.Type tagType = TagType.Type.uncategorized;
                    bool isInParagraphTag = false;
                    if (firstIndex != -1 && closeBracketIndex < chapterContent.Length)
                    {
                        tag = chapterContent.Substring(firstIndex, closeBracketIndex - firstIndex + 1);
                        tagType = TagType.getType(tag);
                        isInParagraphTag = TagType.isInParagraphTag(tag);
                    }
                    ///represents the empty space before paragraphs, in case of div or similar tags; -1 is the default scale
                    ///case 1: there is a tag, and it is at the beginning of the text
                    if (firstIndex == 0 && !isInParagraphTag)
                    {
                        if (buffer.Length != 0)
                        {
                            wordWriter.Paragraph(TagType.removeEndlinesFromEnd(buffer),spaceBefore,spaceAfter);
                            buffer = "";
                            spaceBefore = spaceAfter = -1;
                        }

                        switch (tagType)
                        {
                            case TagType.Type.h1:
                                WriteWarning("Detected header 1. This was not expected");
                                chapterContent = chapterContent.Substring(closeBracketIndex + 1);
                                break;
                            case TagType.Type.h2:
                                WriteWarning("Chapter title has not been handled properly");
                                chapterContent = chapterContent.Substring(closeBracketIndex + 1);
                                break;
                            case TagType.Type.h3:
                                ///TODO - check which one is better
                                string subtitle = TagType.getContent(chapterContent, TagType.Type.h3,out chapterContent);

                                /*tagAndContent = TagType.getTagComplete(chapterContent,out tempString, out chapterContent);
                                string subtitle = tagAndContent.FindFirst("class", "mw-headline").content;*/
                                wordWriter.SubChapter(subtitle);
                                spaceBefore = spaceAfter = -1;
                                break;
                            case TagType.Type.divOpen:
                                string s1;
                                chapterContent = "Ss " + chapterContent;
                                tagAndContent = TagType.getTagComplete(chapterContent, out s1, out chapterContent);
                                chapterContent = TagType.removeEndlinesFromBeginning(chapterContent);
                                string imageLink = tagAndContent.FindFirst("href");
                                //X - imageLink = GetImageURL("File_Ultimate Antihero V2 003.jpg - Baka-Tsuki.html");
                                if (!isLimitedTest)
                                {
                                    imageLink = GetImageURL("https://www.baka-tsuki.org" + imageLink);
                                    Console.WriteLine("Downloading image: " + imageLink);
                                    wordWriter.Image("https://www.baka-tsuki.org" + imageLink);
                                }
                                else
                                {
                                    Console.WriteLine("Got Image Url: " + imageLink);
                                }
                                //X - wordWriter.Image(path + "thumb.png");
                                break;
                            case TagType.Type.ul:
                                try
                                {
                                    string[] listElements = TagType.getNestedContent(chapterContent, TagType.Type.ul, out chapterContent);
                                    wordWriter.BulletList(listElements);
                                }
                                catch
                                {
                                   WriteWarning("Malformed List detected");
                                }
                                break;
                            default:
                                ///TODO this will break for uninterpreted nested tag: use the new method
                                WriteWarning("Uninterpreted tag: " + tag);
                                chapterContent = TagType.removeEndlinesFromBeginning(chapterContent.Substring(closeBracketIndex + 1));
                                break;
                        }
                        
                    }
                    ///case 2: there is a tag, but it is not at the beginning, so the text up to that point is copied
                    else if (firstIndex != -1 && chapterContent.Length != 0)
                    {
                        ///case 2.1: there is a tag, not at the beginning and it is an inparagraph tag,i.e. the paragraph should not be broken
                        if (isInParagraphTag)
                        {
                            string toMod = chapterContent.Substring(0, firstIndex + tag.Length);
                            chapterContent = chapterContent.Substring(firstIndex + tag.Length);
                            switch (tagType)
                            {
                                case (TagType.Type.gt):
                                    toMod = toMod.Replace(tag, ">");
                                    break;
                                case (TagType.Type.lt):
                                    toMod = toMod.Replace(tag, "<");
                                    break;
                                case (TagType.Type.sup):
                                    string bef;
                                    tagAndContent = TagType.getTagComplete(toMod + chapterContent, out bef, out chapterContent);
                                    try {
                                        string footNoteId = tagAndContent.FindFirst("href").Substring(1);
                                        string endNote = footNotes.FindFirst("id",footNoteId).FindFirst("class", "reference-text").content;
                                        wordWriter.ParagraphConditional(buffer + bef);
                                        buffer = "";
                                        toMod = "";
                                        wordWriter.Endnote(endNote);
                                        //Console.WriteLine("Sup id: " + footNoteId + "| content: " + endNote);
                                    }
                                    catch
                                    {
                                        WriteWarning("Footnote reference found, but it points to an invalid location");
                                    }
                                    break;
                                case TagType.Type.span:
                                    toMod = toMod.Replace(tag, "");
                                    string[] spanContent = TagType.getSpanContent(chapterContent, TagType.Type.span, out chapterContent);
                                    chapterContent = chapterContent.Insert(0, spanContent[0] + " [" + spanContent[1] + "]");
                                    break;
                                default:
                                    Console.WriteLine("inParagraph tag has not been handled properly: " + tag);
                                    break;
                            }
                            buffer += toMod;
                            continue;
                        }

                        if (buffer.Length != 0)
                        {
                            chapterContent = buffer + TagType.removeEndlinesFromEnd(chapterContent);
                            firstIndex += buffer.Length;
                            buffer = "";
                        }
                            
                        ///case 2.original, i.e. no inparagraph tag
                        wordWriter.Paragraph(TagType.removeEndlinesFromEnd(chapterContent.Substring(0, firstIndex)),spaceBefore,spaceAfter);
                        spaceBefore = spaceAfter = -1;
                        chapterContent = chapterContent.Substring(firstIndex);
                        
                    }
                    ///case 3 there are no tags, but the text is non-zero length, i.e. it is a tag-free text
                    else if  (firstIndex == -1 && chapterContent.Length != 0)
                    {
                        if (buffer.Length != 0)
                        {
                            chapterContent = buffer + chapterContent;
                            buffer = "";
                        }
                        chapterContent = TagType.removeEndlinesFromEnd(chapterContent);

                        wordWriter.Paragraph(chapterContent,spaceBefore,spaceAfter);
                        spaceBefore = spaceAfter = -1;
                        chapterContent = "";
                    }
                
                }

                Console.WriteLine("Chapter " + (i++-1) + " finished");
                Logger.Lap("Write");

            }

            if (wordWriter.hasEndnotes())
            {
                wordWriter.Chapter("Endnotes");
            }
            wordWriter.TableOfContents(false);
            Logger.Stop("Write");
            Logger.PutDelimiter();
            Console.WriteLine("Writing data to Word document finished");
            wordWriter.SaveAndQuit();
        }

        private string GetImageURL(string url)
        {
            //string html = File.ReadAllText(path + url);
            string html = new System.Net.WebClient().DownloadString(url);
            string content = new StringParser(html).SubstringInclusive("<body","</body>");

            Tag tag = TagType.FastSearch(content, "class=\"fullImageLink\"");
            tag = tag.FindFirst("id", "file");
            return tag.FindFirst("href");
        }

        public void WriteWarning(string message)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        ///TODO Bugs to correct:
        ///1. Get THe proper name of the author.
        ///2. Remove the newline before the References(footnotes)
        ///3. Change the endlines to spaceBefore (i.e. after endnotes)
    }
 }
