using Indiceringsmodule.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.DataAccess
{
    public class ServiceReader : IDocumentReader
    {
        public FlowDocument LoadDocument(string path)
        {
            throw new NotImplementedException();
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
