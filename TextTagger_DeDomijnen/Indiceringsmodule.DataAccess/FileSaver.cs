using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Indiceringsmodule.DataAccess
{
    public class FileSaver : Observable
    {
        private readonly EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        public FileSaver(EventAggregator ea)
        {
            Ea = ea;
            Subscriptions.Add(Ea.Subscribe<PublishDocumentEventModel>(m => OpenSaveFileDialog(m.Data)));
        }

        public void SaveFile()
        {
            Ea.Publish(new RequestDocumentForSavingEventModel() { });
        }


        /// <summary>
        ///
        /// </summary>
        private void OpenSaveFileDialog(DocumentObject docOb)
        {
            //TODO - warning: partially implemented / non-refactored. will break!
            
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Microsoft Word Documents (*.docx)|*.docx|Rich Text Documents (*.rtf)|*.rtf|Json files (*.json)|*.json",
                DefaultExt = ".json"
            };

            var path = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = new DirectoryInfo(path).Parent.Parent.FullName;
            var targetDirectory = rootPath + @"\SavedFiles";

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            saveFileDialog.InitialDirectory = targetDirectory;

            //Ea.Publish(new RequestDocumentForSavingEventModel() { });

            if (saveFileDialog.ShowDialog() == true)
            {
                //TODO - This happens when file is saved/
                var selectedPath = saveFileDialog.FileName;
                Task.Run(() => SaveFileToJson(selectedPath, docOb));
                ValuesChanged = false;
            }
            else
            {
                MessageBox.Show("Error while saving");
            }
        }

        private void SaveFileToJson(string path, DocumentObject docOb)
        {
            var reader = new ServiceReaderJSON();
            reader.SaveDocument(docOb.TranscriptionDocument, path);
        }
    }
}
