using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Baka_Tsuki_Downloader
{
    class Downloader
    {
        static string path = Directory.GetCurrentDirectory() + "\\data\\";

        private static string[] lineSeparators = { "<p>", "\n</p>" };
        private static string[] listSeparators = { "<ul>", "</ul>" };
        private static string[] listElementSeparator = { "<li>", "</li>" };
        private static string[] chapterSeparator = { "<h2>" };

        public Downloader()
        {

        }

        public static void Download(string URL, string fileName)
        {
            WordWriter wordWriter = new WordWriter(fileName);

            string html = new System.Net.WebClient().DownloadString(URL);

            System.IO.File.WriteAllText(fileName, html);

        }
        

        public static void Convert(string sourcefile, string destFile)
        {
            Logger.Start("Read & Convert");
            Console.WriteLine("Reading soulrce file");
            string html = File.ReadAllText(path + sourcefile);
            Console.WriteLine("Creating WordBuilder");
            WordWriter wordWriter = new WordWriter(destFile);

            Console.WriteLine("Creating Parser");
            StringParser content = new StringParser(html);


            StringParser titleHtml = new StringParser(content.Substring("<title>", "</title"));
            string title = titleHtml.SubstringLast(null, ":");
            int volume;
            Int32.TryParse(titleHtml.SubstringLast(":Volume", " - Baka-Tsuki").ToString(), out volume);

            wordWriter.Title(title, "Yuu", volume);


            //TOOD:Handle <Pre>
            content = new StringParser(content.Substring("<h2>", "<td> Back to <a"));

            Console.WriteLine("Starting Conversion");
            string[] chapters = content.ToString().Split(chapterSeparator, StringSplitOptions.RemoveEmptyEntries);

            Logger.Stop("Read & Convert");
            Logger.Start("Write");
            int i = 0;
            foreach (string chapter in chapters)
            {
                if (i >= 3)
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
                                string subtitle = TagType.getContent(chapterContent, TagType.Type.h3,out chapterContent);
                                wordWriter.SubChapter(subtitle);
                                spaceBefore = spaceAfter = -1;
                                break;
                            case (TagType.Type.sup):
                                WriteWarning("Attempting sup");
                                Dictionary<string,string> attribs = TagType.getTagAttributes(tag);
                                string footNote = TagType.getNestedContent(chapterContent, TagType.Type.sup, out chapterContent)[0];
                                wordWriter.Endnote(footNote + "Explanation");

                                break;
                            case TagType.Type.div:
                                spaceBefore += spaceBefore == -1 ? 3 : 1;
                                chapterContent = chapterContent.Substring(closeBracketIndex + 1);
                                chapterContent = TagType.removeEndlinesFromBeginning(chapterContent);
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
                                Console.WriteLine("Uninterpreted tag: " + tag);
                                chapterContent = chapterContent.Substring(closeBracketIndex + 1);
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
                                /*case (TagType.Type.sup):
                                    WriteWarning("Attempting sup");
                                    toMod = toMod.Replace(tag, "");
                                    string footNote = TagType.getNestedContent(chapterContent, TagType.Type.sup, out chapterContent)[0];

                                    wordWriter.Endnote(footNote + "Explanation");
                                    
                                    break;*/
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
                        ///TODO replace this with the method
                        chapterContent = TagType.removeEndlinesFromEnd(chapterContent);

                        wordWriter.Paragraph(chapterContent,spaceBefore,spaceAfter);
                        spaceBefore = spaceAfter = -1;
                        chapterContent = "";
                    }
                
                }

                Console.WriteLine("Chapter " + i++ + " finished");
                Logger.Lap("Write");

            }
            Logger.Stop("Write");
            Logger.PutDelimiter();
            Console.WriteLine("Writing data to Word document finished");
            wordWriter.SaveAndQuit();
        }

        public static void WriteWarning(string message)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
 }
