using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;

namespace Indiceringsmodule.DataAccess
{
    public class DocxToFlowDocumentConverter : ServiceReaderDocx
    {
        //TODO - Attempt to get DocxToFlowDocumentConverter working
        //public FlowDocument document;
        
        //public DocxToFlowDocumentConverter(Stream stream)
        //{

        //}

        //protected override void ReadDocument(XmlReader reader)
        //{
        //    this.document = new FlowDocument();
        //    this.document.BeginInit();
        //    base.ReadDocument(reader);
        //    this.document.EndInit();
        //}

        //protected override void ReadParagraph(XmlReader reader)
        //{
        //    using (this.SetCurrent(new Paragraph()))
        //        base.ReadParagraph(reader);
        //}

        //protected override void ReadRun(XmlReader reader)
        //{
        //    using (this.SetCurrent(new Span()))
        //        base.ReadRun(reader);
        //}

        //private struct CurrentHandle : IDisposable
        //{
        //    private readonly DocxToFlowDocumentConverter converter;
        //    private readonly TextElement previous;
            
        //    public CurrentHandle(DocxToFlowDocumentConverter converter, TextElement current)
        //    {
        //        this.converter = converter;
        //        this.converter.AddChild(current);
        //        this.previous = this.converter.current;
        //        this.converter.current = current;
        //    }

        //    public void Dispose()
        //    {
        //        this.converter.current = this.previous;
        //    }
        //}

        //private TextElement current;

        //private IDisposable SetCurrent(TextElement current)
        //{
        //    return new CurrentHandle(this, current);
        //}
    }
}
