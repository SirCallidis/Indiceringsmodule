using Indiceringsmodule.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Indiceringsmodule.DataAccess
{
    /// <summary>
    /// https://www.codeproject.com/Articles/649064/Show-Word-file-in-WPF
    /// https://www.codeproject.com/Articles/25071/OpenXML-FlowDocument-OpenFlowDocument
    /// </summary>

    public class ServiceReaderDocx : IDocumentReader
    {

        private readonly IFileSystem _fileSystem;

        #region Constructors
        
        //Constructor for production
        public ServiceReaderDocx() : this(new FileSystem()) { }

        //Constructor for test environments
        public ServiceReaderDocx(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens a .docx file through a string path,
        /// converts the file and its contents to an in-memory
        /// FlowDocument and returns the FlowDocument.
        /// </summary>
        /// <param name="path">Full path of the file.</param>
        /// <returns>new FlowDocument populated with the contents of the .docx file.</returns>
        public FlowDocument LoadDocument(string path)
        {
            FlowDocument flowDoc = new FlowDocument();
            Package package = Package.Open(path);
            Uri documentUri = new Uri("/word/document.xml", UriKind.Relative);
            PackagePart documentPart = package.GetPart(documentUri);
            XElement wordDoc = XElement.Load(new StreamReader(documentPart.GetStream()));

            XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            var paragraphs = from p in wordDoc.Descendants(w + "p")
                         select p;

            foreach (var p in paragraphs)
            {
                var style = from s in p.Descendants(w + "pPr")
                            select s;

                var font = (from f in style.Descendants(w + "rFonts")
                            select f.FirstAttribute).FirstOrDefault();
                var size = (from s in style.Descendants(w + "sz")
                            select s.FirstAttribute).FirstOrDefault();

                Paragraph par = new Paragraph();
                Run r = new Run(p.Value);
                if (font != null)
                {
                    FontFamilyConverter converter = new FontFamilyConverter();
                    r.FontFamily = (FontFamily)converter.ConvertFrom(font.Value);
                }
                if (size != null)
                {
                    r.FontSize = double.Parse(size.Value);
                }
                par.Inlines.Add(r);

                flowDoc.Blocks.Add(par);
            }
            return flowDoc;
        }

        /// <summary>
        /// Work in progress.
        /// Program may not need to be able to save as Docx.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="selectedPath"></param>
        /// <returns></returns>
        public Task SaveDocument(FlowDocument document, string selectedPath)
        {
            throw new NotImplementedException();
            //var task = Task.Run(() =>
            //{
            //        using (var inputStreamReader = _fileSystem.File.OpenText(selectedPath))
            //        using (var outputStreamWriter = _fileSystem.File.CreateText(selectedPath))
            //        {
            //            //code to save document as DocX
            //        }
            //});
            //return task;            
        }

        public void Dispose()
        {
        }

        #endregion
    }
}
