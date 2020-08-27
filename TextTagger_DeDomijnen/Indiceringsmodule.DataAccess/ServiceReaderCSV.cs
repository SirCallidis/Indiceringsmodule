using Indiceringsmodule.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.DataAccess
{
    class ServiceReaderCSV : IDocumentReader
    {
        public async Task SaveDocument(FlowDocument document, string selectedPath)
        {
            await Task.Run(() =>
            {
                using (StreamWriter writer = File.CreateText(selectedPath))
                {                   
                    var jsonDocument = JsonConvert.SerializeObject(document);
                    writer.Write(jsonDocument);
                }
            });
        }

        public FlowDocument LoadDocument(string path)
        {
            var fDoc = JsonConvert.DeserializeObject<FlowDocument>
                                (File.ReadAllText(path));
            return fDoc;
        }

        public void Dispose()
        {
        }
    }
}
