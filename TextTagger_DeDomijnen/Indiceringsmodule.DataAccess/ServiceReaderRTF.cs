using Indiceringsmodule.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Indiceringsmodule.DataAccess
{
    public class ServiceReaderRTF : IDocumentReader
    {
        public ServiceReaderRTF()
        {

        }

        public FlowDocument LoadDocument(string path)
        {
            //TODO - Remove link https://stackoverflow.com/questions/14443295/wpf-richtextbox-open-rtf-file-as-plain-text

            var document = new FlowDocument();
            var range = new TextRange(document.ContentStart,document.ContentEnd);
            var fStream = new FileStream(path, FileMode.Open);
            range.Load(fStream, DataFormats.Rtf);
            return document;
        }

        public Task SaveDocument(FlowDocument document, string selectedPath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
