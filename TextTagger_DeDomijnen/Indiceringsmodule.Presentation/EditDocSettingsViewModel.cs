using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Presentation
{
    class EditDocSettingsViewModel : Observable
    {
        #region Field & Properties

        private readonly EventAggregator Ea;

        private Settings _Settings;
        public Settings Settings
        {
            get { return _Settings; }
            set { SetProperty(ref _Settings, value); }
        }

        #endregion

        #region Relay Commands

        public RelayCommand ClosePopUp { get; private set; }

        #endregion

        #region Default Cunstructor

        public EditDocSettingsViewModel(EventAggregator ea)
        {
            Ea = ea;
            ClosePopUp = new RelayCommand(OnClosePopUp, CanClosePopUp);
            Ea.Subscribe<PublishDocSettingsEventModel>(m => { Settings = m.Data; });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true by default. If validation logic is needed, place it here.
        /// </summary>
        /// <returns></returns>
        private bool CanClosePopUp()
        {
            return true;
        }

        /// <summary>
        /// Publishes a ClosePopUp Event, signaling
        /// for the closure of its coupled View.
        /// </summary>
        private void OnClosePopUp()
        {
            if (CanClosePopUp())
            {
                Ea.Publish(new ClosePopUpEventModel() { });
            }
        }

        #endregion
    }
}
