﻿using Microsoft.Office.Interop.Word;
using System;
using System.IO;

namespace Baka_Tsuki_Downloader
{
    //TODO: Delete newline before pageBreaks
    class WordWriter
    {
        static object oMissing = System.Reflection.Missing.Value;
        static object oEndOfDoc = "\\endofdoc"; /* \endofdoc is a predefined bookmark */

        string path;

        Application app;
        Document doc;

        Paragraph parag;
        Range rng;

        Range tocBeginning;

        object start;
        object end;

        string filename;
        private bool isTerminated = false;

        /// <summary>
        /// In order to properly close document, invoking <see cref="SaveAndQuit"/> is necessary
        /// As a general rule, page breaks and new lines will be added after starting a section to the 
        /// previous one, not after finishing a section. This will avoid situations where the new 
        /// line moves the cursor to the next page, then inserts a page break. 
        /// <see cref="Title(string)"/> is an exception, it doesn't have a newline either before or after
        /// Moving the cursor to the end is done at the start of each method, to simplify debugging
        /// </summary>
        /// <param name="title">The filename, not necessarily the title within the document</param>
        public WordWriter(string filename)
        {
            this.filename = filename;

            path = Directory.GetCurrentDirectory() + "\\data\\";///put any path here
            app = new Application();
            app.Visible = false;
            Documents docs = app.Documents;

            docs.Add();// two variables for convinience
            doc = app.ActiveDocument;

            start = 0;
            end = 0;
   
        }

        public void Title(string title)
        {
            Title(title, null, -1);
        }

        public void Title(string title, string author)
        {
            Title(title, author, -1);
        }

        public void Title(string title, string author, int volume)
        {
            parag = doc.Content.Paragraphs.Add(ref oMissing);
            parag.Range.Text = title;
            parag.Format.set_Style("Title");
            parag.Range.Font.Bold = 1;
            parag.Format.SpaceAfter = 20;    //32 pt spacing after paragraph.
            parag.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            parag.Range.InsertParagraphAfter();

            if (volume > 0)
            {
                parag.Range.Text = "Volume " + volume;
                parag.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                parag.Range.Font.Size = 24;
                parag.Format.SpaceAfter = 36;
                parag.Range.InsertParagraphAfter();
            }
            
            if (author != null)
            {
                parag.Range.Text = author;
                parag.Format.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                parag.Range.Font.Color = WdColor.wdColorGray60;
                parag.Range.Font.Italic = 1;
                parag.Range.Font.Size = 22;
                parag.Format.SpaceAfter = 0;
                parag.Range.InsertParagraphAfter();
            }

            parag.Range.InsertBreak(WdBreakType.wdPageBreak);
            tocBeginning = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;
        }

        /// <summary>
        /// Creates a header 1 text and adds a new page before the paragraph
        /// </summary>
        /// <param name="text">The title of the chapter</param>
        public void Chapter(string title)
        {
            parag.Range.InsertBreak(WdBreakType.wdPageBreak);//insert page break (ctrl+enter)
            rng = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;//sets the cursor to the end of the document

            parag = doc.Content.Paragraphs.Add(rng);
            parag.Range.Text = title;
            parag.set_Style("Heading 1");
            
            parag.Format.SpaceAfter = 6;
            parag.Range.InsertParagraphAfter();
        }

        public void SubChapter(string title)
        {
            rng = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;//sets the cursor to the end of the document

            parag = doc.Content.Paragraphs.Add(rng);
            parag.Range.Text = title;
            
            parag.set_Style("Heading 2");
            parag.Format.SpaceAfter = 4;
            parag.Range.InsertParagraphAfter();
        }

        public void Paragraph(string text)
        {
            rng = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;//sets the cursor to the end of the document
            //parag.Range.InsertParagraph();//adds newLine

            parag = doc.Content.Paragraphs.Add(rng);
            parag.Range.Text = text + "\n";
            parag.Range.set_Style("Normal");
            parag.Format.SpaceAfter = 6;
            //parag.Range.InsertParagraphAfter();
        }

        public void Image(string location)
        {

            rng = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;//sets the cursor to the end of the document
            app.Selection.Start = rng.Start;//need to set the cursor of the app.Selection to the end
            app.Selection.set_Style("Normal");
            try
            {
                app.Selection.InlineShapes.AddPicture(location);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            app.Selection.Range.InsertParagraphAfter();
        }



        public void BulletList(string[] items)
        {
            parag = doc.Content.Paragraphs.Add();
            parag.Range.ListFormat.ApplyBulletDefault();
            
            for (int i = 0; i < items.Length; i++)
            {
                string bulletItem = items[i];
                if (i < items.Length - 1)
                    bulletItem = bulletItem + "\n";
                parag.Range.InsertBefore(bulletItem);
            }
        }

        public void Footnote(string text)
        {
        }

        /// <summary>
        /// Choose the place to put the TOC to
        /// </summary>
        /// <param name="atEnd"> true to insert at the end, false to insert after title</param>
        public void TableOfContents(bool atEnd)
        {

            if (atEnd)
            {
                rng = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            }
            else
            {
                rng = tocBeginning; 
            }
                     

            doc.TablesOfContents.Add(rng, true /*use heading styles*/, oMissing, oMissing, oMissing,
                                                    oMissing, oMissing, oMissing, oMissing, oMissing,
                                                    oMissing, oMissing);
            rng.InsertParagraphBefore();
            parag = doc.Content.Paragraphs.Add(tocBeginning);
            parag.Range.Text = "Table of contents";
            parag.Format.set_Style("Heading 1");

        }

        ~WordWriter()
        {
            if (!isTerminated)
            {
                SaveAndQuit();
            }
        }

        public void SaveAndQuit()
        {
            try
            {
                //doc.Save();  //Opens a Save as file dialog  
                doc.SaveAs2(path + filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                app.ActiveDocument.Close();//if the program crashes before this point it is likely the doc will be stuck in an open state
            }
            catch
            {
                Console.WriteLine("Save cancelled");
            }

            app.Quit();
            isTerminated = true;
        }


    }
}
