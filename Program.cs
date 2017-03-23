using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class Program
    {
        static void Main(string[] args)
        {


           /* WriterTest();
            Console.ReadKey(); return;*/

            string title = "[dir]Ultimate Antihero.docx";
            string URL = "https://www.baka-tsuki.org/project/index.php?title=Ultimate_Antihero:Volume_2";
            Downloader.DownloadAndConvert(URL);
            //string html = Downloader.ReadHTML("Ultimate Antihero Volume 2 - Baka-Tsuki.htm");
            //Downloader.Convert(html, title);

            Console.ReadKey();
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
