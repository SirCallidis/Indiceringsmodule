using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.DataAccess;
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
            //create var imtv IndiceringsmoduleTestView with ea (change imtv ctor to accept ea)
            //change MainWindowViewModel.CurrentViewModel to CurrentView
            //Set CurrentView to imtv
            //disable MainWindowView xaml bindings
            //set MainWindowView.xaml's Contentcontrol to CurrentView

            var ea = new EventAggregator();
            var fileLoader = new FileLoader(ea);
            var fileSaver = new FileSaver(ea);
            var menu = new Presentation.Menu(ea, fileLoader, fileSaver);
            var imtvm = new IndiceringsmoduleViewModel(ea, fileLoader, fileSaver);
            var imtv = new IndiceringsmoduleView(ea) { DataContext = imtvm };
            
            new ViewModelCoupler(ea);
            var viewModel = new MainWindowViewModel(ea, menu)
            {
                CurrentView = imtv
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
