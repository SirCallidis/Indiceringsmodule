using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Indiceringsmodule.Presentation
{
    public class MainWindowViewModel : Observable
    {
        #region Fields & Properties

        private readonly EventAggregator Ea;
        private protected Menu Menu;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        //private Observable _CurrentViewModel;
        //public Observable CurrentViewModel
        //{
        //    get { return _CurrentViewModel; }
        //    set { SetProperty(ref _CurrentViewModel, value); }
        //}

        private UserControl _CurrentView;
        public UserControl CurrentView
        {
            get { return _CurrentView; }
            set { SetProperty(ref _CurrentView, value); }
        }

        private UserControl _PopUpWindow;
        public UserControl PopUpWindow
        {
            get { return _PopUpWindow; }
            set { SetProperty(ref _PopUpWindow, value); }
        }

        private bool _PopUpVisible;
        public bool PopUpVisible
        {
            get { return _PopUpVisible; }
            set { SetProperty(ref _PopUpVisible, value); }
        }

        private bool _ContentEnabled;
        public bool ContentEnabled
        {
            get { return _ContentEnabled; }
            set { SetProperty(ref _ContentEnabled, value); }
        }

        private ResourceManager _ReMan;
        public ResourceManager ReMan //possibly use this as the language/culture variable?
        {
            get { return _ReMan; }
            set { SetProperty(ref _ReMan, value); }
        }

        #endregion

        #region RelayCommands

        public RelayCommand Menu_LoadFile { get; private set; }
        public RelayCommand Menu_SaveFile { get; private set; }
        public RelayCommand Menu_CloseFile { get; private set; }
        public RelayCommand Menu_OptionsPopUp { get; private set; }
        public RelayCommand Menu_EditDocSettings { get; private set; }
        #endregion

        #region Default Constructor

        public MainWindowViewModel(EventAggregator ea, Menu menu)
        {
            Ea = ea ?? throw new ArgumentNullException(nameof(ea));
            this.Menu = menu;
            WireUpForm();            
        }

        private void WireUpForm()
        {
            ContentEnabled = true;

            Menu_LoadFile = new RelayCommand(Menu.OnLoadFile, Menu.CanLoadFile);
            Menu_SaveFile = new RelayCommand(Menu.OnSaveFile, Menu.CanSaveFile);
            Menu_CloseFile = new RelayCommand(Menu.OnCloseProgram, Menu.CanCloseProgram);
            Menu_OptionsPopUp = new RelayCommand(OnOptionsPopUp, CanOptionsPopUp);
            Menu_EditDocSettings = new RelayCommand(Menu.OnEditDocSettings, Menu.CanEditDocSettings);

            Subscriptions.Add(Ea.Subscribe<ProvidingViewForViewModelEventModel> (m => ResolveView(m.Data)));
            Subscriptions.Add(Ea.Subscribe<ClosePopUpEventModel>(m => ResolvePopUpVisibility()));
            Subscriptions.Add(Ea.Subscribe<LanguageChangedEventModel>(m => SetLanguageDictionary(m.Data)));
        }

        #endregion

        /// <summary>
        /// Placeholder for validation logic.
        /// For now returns true by default.
        /// </summary>
        /// <returns></returns>
        private bool CanOptionsPopUp()
        {
            return true;
        }

        private void OnOptionsPopUp()
        {
            //check if operation is allowed
            //set PopUpWindow to instance of relevant popup user control
            //toggle PopUpVisible
            //enable or disable content based on popupvisibility
            
            if (CanOptionsPopUp())
            {
                var popUpOptionsVM = new PopUpOptionsViewModel(Ea);
                Ea.Publish(new RequestViewForViewModelEventModel() { Data = popUpOptionsVM });
            }
        }

        private void ResolveView(object data)
        {
            var view = data as UserControl;
            PopUpWindow = view;
            ResolvePopUpVisibility();
        }

        private void ResolvePopUpVisibility()
        {
            PopUpVisible ^= true;
            if (PopUpVisible)
            {
                ContentEnabled = false;
            }
            else
            {
                ContentEnabled = true;
            }
        }

        private void SetLanguageDictionary(string selectedLanguage)
        {
            //TODO: Implement Set Language

            ////below: doesn't work
            //var ass = Assembly.GetExecutingAssembly();
            //var rm = new ResourceManager("Indiceringsmodule.Language", Assembly.GetExecutingAssembly());
            ////var resu = rm.GetString("MenuQuit");

            ////below: does work
            //var reman = Resources.ResourceManager;
            //var result = reman.GetString("MenuQuit");
            
            //var culture = Thread.CurrentThread.CurrentCulture.ToString();

            //switch (selectedLanguage)
            //{
            //    case "Nederlands":
            //        Resources.Culture = new CultureInfo("nl-NL");
            //        //more code here to set which .resx file is used                   
            //        break;
            //    case "English":
            //        Language.Resources.Culture = new CultureInfo("en-GB");
            //        break;
            //    case "Deutsch":
            //        Language.Resources.Culture = new CultureInfo("de-DE");
            //        break;
            //    default:
            //        Language.Resources.Culture = new CultureInfo("en-GB");
            //        break;
            //}
        }
    }
}
