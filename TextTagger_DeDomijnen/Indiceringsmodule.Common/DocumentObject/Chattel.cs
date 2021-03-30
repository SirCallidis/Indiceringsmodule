using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Chattel : FactMember
    {
        #region Fields & Properties

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set { SetProperty(ref _Type, value); }
        }

        private string _Worth;
        public string Worth
        {
            get { return _Worth; }
            set { SetProperty(ref _Worth, value); }
        }

        private string _Valuta;
        public string Valuta
        {
            get { return _Valuta; }
            set { SetProperty(ref _Valuta, value); }
        }

        #endregion


        #region Default Constructor

        public Chattel(int id, Hyperlink link, EventAggregator ea)
        {
            ID = id;
            Link = link;
            Ea = ea;
            WireUpFactMember();
            Ea.Publish(new RequestExtraSetsEventModel() { });
        }

        #endregion
    }
}
