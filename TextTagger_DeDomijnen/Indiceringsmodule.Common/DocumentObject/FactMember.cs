using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
