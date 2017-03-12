using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        /*public static void ConvertOld(string sourcefile, string destFile)
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
                wordWriter.Chapter("Prologue");

                //inserting everything at once with \n instead of paragraph by paragraph increased speed by ~1000 times
                //string c = chapter.Replace("<p>", "").Replace("</p>","").Replace("<br />","\n");
                // wordWriter.Paragraph(c);

                List<string> chapterlist = chapter.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
                chapterlist.ForEach(s => s.Split(chapterSeparator, StringSplitOptions.RemoveEmptyEntries));


                foreach (string line in chapterlist)
                {
                    if (line.Contains("<br />") || line.Contains("<br/>"))
                    {
                        string l = line.Replace("<br />", "");
                        wordWriter.Paragraph(l);
                        wordWriter.Paragraph("");//need to adjust space after instead
                    }
                    else
                    {
                        wordWriter.Paragraph(line);
                    }
                }
                Console.WriteLine("Chapter " + i++ + "finished");
                Logger.Lap("Write");

            }
            Logger.Stop("Write");
            Logger.PutDelimiter();
            Console.WriteLine("Writing data to Word document finished");
            wordWriter.SaveAndQuit();
        }*/

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


                string chapterTitle = chapterContent.Substring(chapterContent.IndexOf('>') + 1);
                chapterTitle = chapterTitle.Substring(0, chapterTitle.IndexOf('<'));
                wordWriter.Chapter(chapterTitle); 
                chapterContent = chapterContent.Substring(chapterContent.IndexOf("</h2>") + 5);


                //TODO handle & for italic etc. tags 

                while (chapterContent.Length != 0)
                {
                    int firstIndex = chapterContent.IndexOf('<');

                    if (firstIndex == 0)
                    {
                        switch (chapterContent[1])
                        {
                            case 'h':
                                switch (chapterContent[2])
                                {
                                    case '2':
                                        Console.WriteLine("chapter title has not been handled properly");
                                        break;
                                    default:
                                        Console.WriteLine("Unrecognized header: " + chapterContent[2]);
                                        ///TODO this only handles single digit headers
                                        chapterContent = chapterContent.Substring(chapterContent.IndexOf("</h" + chapterContent[2] + ">") + 5);
                                        break;
                                }
                                break;
                            default:
                                int closeBracketIndex = chapterContent.IndexOf('>');
                                closeBracketIndex = closeBracketIndex != -1 ? closeBracketIndex : chapterContent.Length;
                                Console.WriteLine("Uninterpreted tag: " + chapterContent.Substring(0, closeBracketIndex + 1));
                                chapterContent = chapterContent.Substring(closeBracketIndex + 1);
                                break;
                        }
                        firstIndex = chapterContent.IndexOf('<');
                    }
                    else if (firstIndex != -1 && chapterContent.Length != 0)
                    {
                        wordWriter.Paragraph(chapterContent.Substring(0, firstIndex));
                        chapterContent = chapterContent.Substring(firstIndex);
                    }
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
