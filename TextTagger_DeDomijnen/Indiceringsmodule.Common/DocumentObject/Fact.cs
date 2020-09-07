using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Fact : Observable
    {
        #region Fields & Properties

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

        public Fact()
        {
        }

        #endregion
    }
}
