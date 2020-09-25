using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Chattel : FactMember
    {
        #region Fields & Properties
        
        #endregion


        #region Default Constructor

        public Chattel(int id, string linkSelection)
        {
            ID = id;
            LinkSelection = linkSelection;
        }

        #endregion
    }
}
