using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.Presentation;
using Indiceringsmodule.WPFViews;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Indiceringsmodule
{
    public partial class App : Application
    {   
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);            
            ComposeObjects();
            SetLanguageDictionary();            
            Current.MainWindow.Show();
        }

        private static void ComposeObjects()
        {
            var ea = new EventAggregator();
            var menu = new Presentation.Menu(ea);
            new ViewModelCoupler(ea);
            var viewModel = new MainWindowViewModel(ea, menu)
            {
                CurrentViewModel = new IndiceringsmoduleTestViewModel(ea)
            };
            Current.MainWindow = new MainWindow(ea, viewModel);
        }

        private void SetLanguageDictionary()
        {
            var culture = Thread.CurrentThread.CurrentCulture.ToString();
            switch (culture)
            {
                case "nl-NL":
                    Language.Resources.Culture = new CultureInfo("nl-NL");
                    break;
                case "en-NL":
                    Language.Resources.Culture = new CultureInfo("en-NL");
                    break;
                case "en-GB":
                    Language.Resources.Culture = new CultureInfo("en-GB");
                    break;
                default:
                    Language.Resources.Culture = new CultureInfo("en-GB");
                    break;
            }
        }
    }
}
