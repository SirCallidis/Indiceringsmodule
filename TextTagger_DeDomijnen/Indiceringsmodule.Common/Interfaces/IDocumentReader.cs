using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.Interfaces
{
    public interface IDocumentReader : IDisposable
    {
        FlowDocument LoadDocument(string path);
        Task SaveDocument(FlowDocument document, string selectedPath);
    }
}
