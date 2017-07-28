using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class Program
    {
        private static Downloader downloader;
        /// <summary>
        /// TODO - This should obviously be fethced, or be included on install
        /// </summary>
        static readonly string calibreLoc = @"C:\Program Files\Calibre2";
        static DocxToMobiConverter docxToMobi;
        static void Main(string[] args)
        {
            docxToMobi = new DocxToMobiConverter(calibreLoc);

            /* WriterTest();
             Console.ReadKey(); return;*/

            string resultingFile = /*"[dir]Ultimate Antihero.docx";*/ LimitedTest();
            //resultingFile = CompleteTest();

            Console.WriteLine("Successfully saved to " + resultingFile);
            Console.WriteLine("Press any key to convert the docx to .mobi. Press escape to Terminate program");
            System.ConsoleKeyInfo k = Console.ReadKey();
            if (!k.Key.Equals(ConsoleKey.Escape))
            {
                Console.WriteLine("Starting Conversion");
                docxToMobi.Convert(resultingFile);
                Console.WriteLine("Conversion Finished");
                Console.ReadKey();
            }
        }

        public static string CompleteTest()
        {
            downloader = new Downloader();
            Console.WriteLine("Please specify the URL to download from: ");
            string URL = Console.ReadLine(); // "https://www.baka-tsuki.org/project/index.php?title=Ultimate_Antihero:Volume_2";
            downloader.setAuthor("Riku Misora|海空 りく");
            return downloader.DownloadAndConvert(URL);
        }

        public static string LimitedTest()
        {
            downloader = new Downloader(true);
            downloader.setAuthor("Riku Misora|海空 りく");
            string html = downloader.ReadHTML("Ultimate_Antihero Volume_3.html");
            string title = "[dir]Ultimate Antihero.docx";
            return downloader.Convert(html,title);
        }

        public static void WriterTest()
        {
            WordWriter wordWriter = new WordWriter("Seireitsukai.docx");
            wordWriter.Title("Seireitsukai no Blade Dance", "Shimizu Yuu", 7);

            wordWriter.Chapter("Chapter 1: Elysium");
            wordWriter.Paragraph("Text 1 in parag 1");
            wordWriter.SubChapter("Part 2");
            wordWriter.Paragraph("text 1 in parag 2");
            wordWriter.BulletList(new string[]{ "ONe","Two","Three"});
            wordWriter.Chapter("Chapter 2: w");

            wordWriter.Paragraph("Text 2 in parag 1");
            wordWriter.Image("https://www.google.com/logos/doodles/2016/thanksgiving-2016-5674020369334272-hp2x.jpg");
            wordWriter.Paragraph("text 2 in parag 2");

            wordWriter.TableOfContents(false);
        }
    }
}
