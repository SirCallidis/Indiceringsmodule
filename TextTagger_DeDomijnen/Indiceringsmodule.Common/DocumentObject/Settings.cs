using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Settings : Observable
    {
        #region Fields & Properties

        private string _SourceID;
        public string SourceID
        {
            get { return _SourceID; }
            set { SetProperty(ref _SourceID, value); }
        }

        private string _ADACode;
        public string ADACode
        {
            get { return _ADACode; }
            set { SetProperty(ref _ADACode, value); }
        }

        private string _Location;
        public string Location
        {
            get { return _Location; }
            set { SetProperty(ref _Location, value); }
        }

        private string _LocationCode;
        public string LocationCode
        {
            get { return _LocationCode; }
            set { SetProperty(ref _LocationCode, value); }
        }

        private string _Municipality;
        public string Municipality
        {
            get { return _Municipality; }
            set { SetProperty(ref _Municipality, value); }
        }

        private string _SourceType;
        public string SourceType
        {
            get { return _SourceType; }
            set { SetProperty(ref _SourceType, value); }
        }

        private string _FactType;
        public string FactType
        {
            get { return _FactType; }
            set { SetProperty(ref _FactType, value); }
        }

        private string _FactSubType;
        public string FactSubType
        {
            get { return _FactSubType; }
            set { SetProperty(ref _FactSubType, value); }
        }

        #endregion Fields & Properties

        #region Default Constuctor

        public Settings()
        {
        }

        #endregion Default Constuctor
    }
}
