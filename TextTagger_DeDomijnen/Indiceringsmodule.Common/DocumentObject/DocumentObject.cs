using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class DocumentObject : Observable
    {
        #region Fields & Properties

        //Raw file loaded into app containing text and images
        private FlowDocument _LoadedFlowDocument; 
        public FlowDocument LoadedFlowDocument
        {
            get { return _LoadedFlowDocument; }
            set { SetProperty(ref _LoadedFlowDocument, value); }
        }

        //document for selection and editing, containing Fact links
        private FlowDocument _TranscriptionDocument; 
        public FlowDocument TranscriptionDocument
        {
            get { return _TranscriptionDocument; }
            set { SetProperty(ref _TranscriptionDocument, value); }
        }

        //Contains fields for information related to the external source material
        //of the DocumentObject
        private protected Settings _Settings;
        public Settings Settings
        {
            get { return _Settings; }
            set { SetProperty(ref _Settings, value); }
        }

        private List<Fact> _TotalFacts;
        public List<Fact> TotalFacts
        {
            get { return _TotalFacts; }
            set { SetProperty(ref _TotalFacts, value); }
        }

        private List<Fact> _FactSubGroup;
        public List<Fact> FactSubGroup
        {
            get { return _FactSubGroup; }
            set { SetProperty(ref _FactSubGroup, value); }
        }



        #endregion

        #region Default Constructor

        public DocumentObject()
        {
            Settings = new Settings();
        }

        #endregion
    }
}
