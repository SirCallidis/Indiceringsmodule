using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Presentation
{
    public class PopUpOptionsViewModel : Observable
    {
        private protected EventAggregator Ea;

        private string _SelectedLanguage;
        public string SelectedLanguage
        {
            get { return _SelectedLanguage; }
            set { SetProperty(ref _SelectedLanguage, value); }
        }

        private List<string> _Languages;
        public List<string> Languages
        {
            get { return _Languages; }
            set { SetProperty(ref _Languages, value); }
        }


        public RelayCommand ClosePopUp { get; private set; }
        public RelayCommand SelectedLanguageChanged { get; private set; }


        public PopUpOptionsViewModel(EventAggregator ea)
        {
            Ea = ea;
            WireUpForm();
        }

        private void WireUpForm()
        {
            ClosePopUp = new RelayCommand(OnClosePopUp, CanClosePopUp);
            SelectedLanguageChanged = new RelayCommand(OnSelectedLanguageChanged, CanSelectedLanguageChanged);
            Languages = new List<string> { "English", "Deutsch", "Nederlands" };
        }

        private bool CanSelectedLanguageChanged()
        {
            if (SelectedLanguage != null)
            {
                return true;
            }
            return false;
        }

        private void OnSelectedLanguageChanged()
        {
            if (CanSelectedLanguageChanged())
            {
                Ea.Publish(new LanguageChangedEventModel() { Data = SelectedLanguage });
            }
        }

        private bool CanClosePopUp()
        {
            return true;
        }

        private void OnClosePopUp()
        {
            if (CanClosePopUp())
            {
                Ea.Publish(new ClosePopUpEventModel() { });
            }
        }
    }
}
