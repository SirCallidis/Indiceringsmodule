using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class RealEstate : Observable
    {
        #region Fields & Properties

        private string _Location;
        public string Location
        {
            get { return _Location; }
            set { SetProperty(ref _Location, value); }
        }

        private string _Toponym;
        public string Toponym
        {
            get { return _Toponym; }
            set { SetProperty(ref _Toponym, value); }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set { SetProperty(ref _Type, value); }
        }

        private string _Size;
        public string Size
        {
            get { return _Size; }
            set { SetProperty(ref _Size, value); }
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

        private string _Remark;
        public string Remark
        {
            get { return _Remark; }
            set { SetProperty(ref _Remark, value); }
        }

        #endregion

        #region Default Constructor

        public RealEstate()
        {
        }

        #endregion
    }
}
