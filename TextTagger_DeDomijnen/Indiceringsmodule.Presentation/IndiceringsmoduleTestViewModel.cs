using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Indiceringsmodule.Presentation
{
    public class IndiceringsmoduleTestViewModel : Observable, IDisposable
    {
        #region Fields & Properties

        public readonly EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        private DocumentObject _DocumentObject;
        public DocumentObject DocumentObject
        {
            get { return _DocumentObject; }
            set { SetProperty(ref _DocumentObject, value); }
        }



        //private FlowDocument _CurrentDocument;
        //public FlowDocument CurrentDocument
        //{
        //    get { return _CurrentDocument; }
        //    set { SetProperty(ref _CurrentDocument, value); }
        //}
        //private FlowDocument _EditableDocument;
        //public FlowDocument EditableDocument
        //{
        //    get { return _EditableDocument; }
        //    set { SetProperty(ref _EditableDocument, value); }
        //}

        #endregion


        #region Relay Commands

        public RelayCommand Button_ClearLeft { get; private set; }

        #endregion

        #region Default Constructor

        public IndiceringsmoduleTestViewModel(EventAggregator ea)
        {
            Ea = ea;           
            WireUpForm();
        }

        private void WireUpForm()
        {
            Subscriptions.Add(Ea.Subscribe<DocumentLoadedEventModel>(m => DisplayNewCurrentDocument(m.Document)));
            Subscriptions.Add(Ea.Subscribe<RequestDocumentForSavingEventModel>(m => MakeDocumentAvailable()));
            Subscriptions.Add(Ea.Subscribe<RequestDocSettingsEventModel>(m => MakeDocSettingsAvailable()));
            
            Button_ClearLeft = new RelayCommand(OnClearLeft, CanClearLeft);
            DocumentObject = new DocumentObject
            {
                TranscriptionDocument = new FlowDocument()
            };

            //below: some test stuff
            Paragraph par = new Paragraph();
            Run run1 = new Run("Programmatically added text.");
            par.Inlines.Add(run1);
            DocumentObject.TranscriptionDocument.Blocks.Add(par);
        }



        #endregion

        /// <summary>
        /// Checks wether the CurrentDocument property is null,
        /// if it is, it sets the document parameter as the new CurrentDocument.
        /// If it isn't, it'll append the document parameter to the existing
        /// CurrentDocument.
        /// </summary>
        /// <param name="document"></param>
        private void DisplayNewCurrentDocument(FlowDocument document)
        {
            if (DocumentObject.LoadedFlowDocument == null)
            {
                DocumentObject.LoadedFlowDocument = document;
            }
            else
            {
                for (int i = 0; i < document.Blocks.Count; i++)
                {
                    DocumentObject.LoadedFlowDocument.Blocks.Add(document.Blocks.ElementAt(i));
                }
            }           
        }

        private bool CanClearLeft()
        {
            return true;
        }

        private void OnClearLeft()
        {
            if (CanClearLeft())
            {
                DocumentObject.LoadedFlowDocument = null;
            }
        }

        private void MakeDocumentAvailable()
        {
            Ea.Publish(new PublishDocumentEventModel() { Document = DocumentObject.TranscriptionDocument });
        }

        private void MakeDocSettingsAvailable()
        {
            Ea.Publish(new PublishDocSettingsEventModel() { Data = DocumentObject.Settings });
        }

        public void Dispose()
        {
            foreach (var sub in Subscriptions)
            {
                sub?.Dispose();
            }
        }
    }
}
