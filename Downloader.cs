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
            string html = File.ReadAllText(path + sourcefile);
            Console.WriteLine("File read");
            WordWriter wordWriter = new WordWriter(destFile);
            Console.WriteLine("WordBuilder created");

            StringParser content = new StringParser(html);


            StringParser titleHtml = new StringParser(content.Substring("<title>", "</title"));
            string title = titleHtml.SubstringLast(null, ":");
            int volume;
            Int32.TryParse(titleHtml.SubstringLast(":Volume", " - Baka-Tsuki").ToString(), out volume);

            wordWriter.Title(title, "Yuu", volume);


            //TOOD:Handle <Pre>, <li>, images, furigana substitute
            content = new StringParser(content.Substring("<h2>", "<td> Back to <a"));

            //StringParser Chapter = new StringParser(content.Substring(null, "<h2>"));
            //Chapter = new StringParser(Chapter.Substring("<p>", null));
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
                string chapterContent = chapter.Replace("<p>", "").Replace("</p>","").Replace("<br />","\n");
                //wordWriter.Paragraph(chapter);


                string chapterTitle = TagType.getContent(chapterContent, TagType.Type.h2, out chapterContent);
                wordWriter.Chapter(chapterTitle);


                //TODO handle & for italic etc. tags 

                while (chapterContent.Length != 0)
                {
                    int firstIndex = chapterContent.IndexOf('<');

                    ///case 1: there is a tag, and it is at the beginning of the text
                    if (firstIndex == 0)
                    {
                        int closeBracketIndex = chapterContent.IndexOf('>');
                        closeBracketIndex = closeBracketIndex != -1 ? closeBracketIndex : chapterContent.Length;
                        string tag = chapterContent.Substring(0, closeBracketIndex + 1);

                        switch (TagType.getType(tag))
                        {
                            case TagType.Type.h1:
                                Console.Write("Detected header 1. This was not expected");
                                break;
                            case TagType.Type.h2:
                                Console.WriteLine("chapter title has not been handled properly");
                                break;
                            /*case TagType.Type.h3:
                                string subtitle = TagType.getContent(chapterContent, TagType.Type.h3,out chapterContent);
                                wordWriter.SubChapter(subtitle);
                                break;*/
                            default:
                                Console.WriteLine("Uninterpreted tag: " + tag);
                                chapterContent = chapterContent.Substring(closeBracketIndex + 1);
                                break;
                        }
                        
                    }
                    ///case 2: there is a tag, but it is not at the beginning, so the text up to that point is copied
                    else if (firstIndex != -1 && chapterContent.Length != 0)
                    {
                        wordWriter.Paragraph(chapterContent.Substring(0, firstIndex));
                        chapterContent = chapterContent.Substring(firstIndex);
                    }
                    ///case 3 there are no tags, but the text is non-zero length, i.e. it is a tag-free text
                    else if  (firstIndex == -1 && chapterContent.Length != 0)
                    {
                        wordWriter.Paragraph(chapterContent);
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
    }
 }
