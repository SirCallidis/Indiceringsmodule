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
        #region Fields & Properties

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

        #endregion Fields & Properties

        #region RelayCommands

        public RelayCommand ClosePopUp { get; private set; }
        public RelayCommand SelectedLanguageChanged { get; private set; }

        #endregion RelayCommands

        #region Default Constructor

        public PopUpOptionsViewModel(EventAggregator ea)
        {
            Ea = ea;
            WireUpForm();
        }

        /// <summary>
        /// Wires up all the parts of the class
        /// </summary>
        private void WireUpForm()
        {
            ClosePopUp = new RelayCommand(OnClosePopUp, CanClosePopUp);
            SelectedLanguageChanged = new RelayCommand(OnSelectedLanguageChanged, CanSelectedLanguageChanged);
            
            //TODO: language list is currently hardcoded
            Languages = new List<string> { "English", "Deutsch", "Nederlands" };
        }

        #endregion Default Constructor

        #region Methods

        /// <summary>
        /// Validates if the selected language can be changed.
        /// </summary>
        /// <returns></returns>
        private bool CanSelectedLanguageChanged()
        {
            if (SelectedLanguage != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Publishes an event presenting the selected language
        /// </summary>
        private void OnSelectedLanguageChanged()
        {
            if (CanSelectedLanguageChanged())
            {
                Ea.Publish(new LanguageChangedEventModel() { Data = SelectedLanguage });
            }
        }

        /// <summary>
        /// returns true by default. If additional validation is required, place it here.
        /// </summary>
        /// <returns></returns>
        private bool CanClosePopUp()
        {
            return true;
        }

        /// <summary>
        /// Fires an event to close an active popup.
        /// </summary>
        private void OnClosePopUp()
        {
            if (CanClosePopUp())
            {
                Ea.Publish(new ClosePopUpEventModel() { });
            }
        }

        #endregion Methods
    }
}
