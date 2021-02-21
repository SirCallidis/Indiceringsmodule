using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Indiceringsmodule.Common.DocumentObject
{
    public partial class FactMember : Observable
    {
        private protected int _ID;
        public int ID
        {
            get { return _ID; }
            set { SetProperty(ref _ID, value); }
        }

        //represents the sequence of characters that form the
        //clickable hyperlink inside the FactDocument
        private string _LinkSelection;
        public string LinkSelection
        {
            get { return _LinkSelection; }
            set { SetProperty(ref _LinkSelection, value); }
        }

        //represents the key by which the FactMember
        //can be looked up in the Fact's Dictionairy.
        //May override the above LinkSelection completely!
        private Hyperlink _Link;
        public Hyperlink Link
        {
            get { return _Link; }
            set { SetProperty(ref _Link, value); }
        }

        private string _Remark;
        public string Remark
        {
            get { return _Remark; }
            set { SetProperty(ref _Remark, value); }
        }

        private ExpandoObject _Details;
        public ExpandoObject Details
        {
            get { return _Details; }
            set { SetProperty(ref _Details, value); }
        }
    }
}
