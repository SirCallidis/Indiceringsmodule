using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class DocumentObject : Observable
    {
        #region Fields & Properties

        public readonly EventAggregator Ea;

        //Contains fields for information related to the external source material
        //of the DocumentObject
        private protected Settings _Settings;
        public Settings Settings
        {
            get { return _Settings; }
            set { SetProperty(ref _Settings, value); }
        }

        private ObservableCollection<Fact> _TotalFacts;
        public ObservableCollection<Fact> TotalFacts
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

        #endregion Fields & Properties


        #region Default Constructor

        /// <summary>
        /// Creates a new DocumentObject containing instantiated, but empty:
        /// -Settings
        /// -TotalFactsList
        /// </summary>
        public DocumentObject(EventAggregator ea)
        {
            Ea = ea;
            Settings = new Settings();            
            FactSubGroup = new List<Fact>();
            TotalFacts = new ObservableCollection<Fact>();
            Images = new ObservableDictionary<string, BitmapImage>();
        }
        #endregion Default Constructor

        #region Methods

        /// <summary>
        /// Checks the number of facts in the TotalFacts list,
        /// based off that, instantiates a new Fact with the
        /// subsequent ID number and incoming string selection.
        /// </summary>
        public void CreateFact(string selection)
        {
            var newID = TotalFacts.Count();
            TotalFacts.Add(new Fact(newID, selection, Ea));
        }

        /// <summary>
        /// Returns true if the DocumentObject contains a fact with a lower ID number,
        /// or false if it doesn't.
        /// </summary>
        /// <param name="givenID"></param>
        /// <returns></returns>
        public bool IsThereALowerFactID(int givenID)
        {
            var lowestIDnumber = TotalFacts.Min(f => f.ID);
            if (givenID == lowestIDnumber)
            {
                return false;
            }
            if (givenID < lowestIDnumber)
            {
                throw new IndexOutOfRangeException($"*Could not process Fact ID {givenID} in sequence of count: {lowestIDnumber}");
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the DocumentObject contains a fact with a higher ID number,
        /// or false if it doesn't.
        /// </summary>
        /// <param name="givenID"></param>
        /// <returns></returns>
        public bool IsThereAHigherFactID(int givenID)
        {
            var highestIDnumber = TotalFacts.Max(f => f.ID);
            if (givenID == highestIDnumber)
            {
                return false;
            }
            if (givenID > highestIDnumber)
            {
                throw new IndexOutOfRangeException($"*Could not process Fact ID {givenID} in sequence of count: {highestIDnumber}");
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Validates the data in the DocumentObject and returns a class containing the findings
        /// </summary>
        /// <returns></returns>
        public DocumentObjectValidationFindings Validate()
        {
            var docObVal = new DocumentObjectValidationFindings();
            foreach (var fact in TotalFacts)
            {
                var findings = fact.Validate();
                if (!findings.allGreen)
                {
                    docObVal.nonGreenFactsFindings.Add(findings);
                }
            }
            if (docObVal.nonGreenFactsFindings.Count == 0)
            {
                docObVal.allGreen = true;
                return docObVal;
            }
            return docObVal;
        }

        #endregion Methods
    }
}
