using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class DocumentObject : Observable
    {
        #region Fields & Properties

        //Raw file loaded into app containing text and images
        // No longer needed as the images are loaded in the viewmodel
        //private FlowDocument _LoadedFlowDocument; 
        //public FlowDocument LoadedFlowDocument
        //{
        //    get { return _LoadedFlowDocument; }
        //    set { SetProperty(ref _LoadedFlowDocument, value); }
        //}

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

        private ObservableDictionary<string, BitmapImage> _Images;
        public ObservableDictionary<string, BitmapImage> Images
        {
            get { return _Images; }
            set { SetProperty(ref _Images, value); }
        }

        private FlowDocument _TranscriptionDocument;
        public FlowDocument TranscriptionDocument
        {
            get { return _TranscriptionDocument; }
            set { SetProperty(ref _TranscriptionDocument, value); }
        }

        #endregion


        #region Default Constructor

        /// <summary>
        /// Creates a new DocumentObject containing instantiated, but empty:
        /// -Settings
        /// -TotalFactsList
        /// </summary>
        public DocumentObject()
        {
            Settings = new Settings();            
            FactSubGroup = new List<Fact>();
            TotalFacts = new List<Fact>();
            Images = new ObservableDictionary<string, BitmapImage>();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Checks the number of facts in the TotalFacts list,
        /// based off that, instantiates a new Fact with the
        /// subsequent ID number and incoming string selection.
        /// </summary>
        public void CreateFact()
        {
            var newID = TotalFacts.Count();
            TotalFacts.Add(new Fact(newID));
        }

        #endregion
    }
}
