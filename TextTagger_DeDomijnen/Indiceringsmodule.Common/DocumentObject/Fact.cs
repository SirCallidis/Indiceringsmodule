using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Fact : Observable
    {
        #region Fields & Properties

        private protected int _ID;
        public int ID { get { return _ID; } }

        //document for selection and editing, containing (only)
        //text and links pertaining to a single fact
        private FlowDocument _FactDocument;
        public FlowDocument FactDocument
        {
            get { return _FactDocument; }
            set { SetProperty(ref _FactDocument, value); }
        }

        //a list containing all FactMembers of the current fact,
        //each with a unique ID
        private List<FactMember> _TotalFactMembers;
        public List<FactMember> TotalFactMembers
        {
            get { return _TotalFactMembers; }
            set { SetProperty(ref _TotalFactMembers, value); }
        }

        private Person _Person;
        public Person Person
        {
            get { return _Person; }
            set { SetProperty(ref _Person, value); }
        }

        private RealEstate _RealEstate;
        public RealEstate RealEstate
        {
            get { return _RealEstate; }
            set { SetProperty(ref _RealEstate, value); }
        }

        private Chattel _Chattel;
        public Chattel Chattel
        {
            get { return _Chattel; }
            set { SetProperty(ref _Chattel, value); }
        }

        #endregion

        #region Default Constructor

        public Fact(int id)
        {
            _ID = id;
            FactDocument = new FlowDocument();
        }

        #endregion

        #region Methods

        public void CreatePerson(string selection)
        {
            var newID = TotalFactMembers.Count();
            TotalFactMembers.Add(new Person(newID, selection));
        }

        public void CreateRealEstate(string selection)
        {
            var newID = TotalFactMembers.Count();
            TotalFactMembers.Add(new RealEstate(newID, selection));
        }

        public void CreateChattel(string selection)
        {
            var newID = TotalFactMembers.Count();
            TotalFactMembers.Add(new Chattel(newID, selection));
        }

        #endregion
    }
}
