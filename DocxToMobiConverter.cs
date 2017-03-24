using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class DocxToMobiConverter
    {
        string calibreLocation;
        public DocxToMobiConverter(string calibreLocation)
        {
            this.calibreLocation = calibreLocation;
            if (calibreLocation[calibreLocation.Length - 1] != '\\')
            {
                this.calibreLocation += '\\';
            }
        }

        public void Convert(string docxFileName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = calibreLocation + "ebook-convert.exe";
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = '\"' + Directory.GetCurrentDirectory() + "\\data\\" + docxFileName + '\"' + " .mobi " + "--disable-remove-fake-margins";

            Process calibreProcess = new Process();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                
                calibreProcess = Process.Start(startInfo);
                string output = calibreProcess.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
                string err = calibreProcess.StandardError.ReadToEnd();
                Console.WriteLine(err);
                calibreProcess.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
