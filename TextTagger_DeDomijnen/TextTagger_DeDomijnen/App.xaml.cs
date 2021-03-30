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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Indiceringsmodule
{
    public partial class App : System.Windows.Application
    {
        private static string Path  { get; set; }
        private static string RootPath  { get; set; }
        private static string LanguagePath { get; set; }
        private static ResXResourceSet LangResource { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Path = AppDomain.CurrentDomain.BaseDirectory;
            RootPath = new DirectoryInfo(Path).Parent.Parent.Parent.FullName;
            LanguagePath = RootPath + @"\Indiceringsmodule.Language";       
            SetLanguageDictionary();
            ComposeObjects();           
            Current.MainWindow.Show();
        }

        /// <summary>
        /// assembles all the high-level pieces to create a working app
        /// </summary>
        private static void ComposeObjects()
        {
            var rm = new ResourceManager((LanguagePath + @"\Resources"), Assembly.GetExecutingAssembly());
            //var x = rm.GetString("MenuLoad"); //breaks because Resx file needs to be converted to .Resources (look into this when language selection has to be made dynamic)
            var ea = new EventAggregator();
            var fileLoader = new FileLoader(ea);
            var fileSaver = new FileSaver(ea);
            var menu = new Presentation.Menu(ea, fileLoader, fileSaver);
            
            var imvm = new IndiceringsmoduleViewModel(ea, fileLoader, fileSaver);
            var imv = new IndiceringsmoduleView(ea) { DataContext = imvm };
            
            new ViewModelCoupler(ea);
            var viewModel = new MainWindowViewModel(ea, menu, LangResource)
            {
                CurrentView = imv
            };
            Current.MainWindow = new MainWindow(viewModel);
        }

        /// <summary>
        /// sets the language set based on the current culture.
        /// </summary>
        private void SetLanguageDictionary()
        {
            var culture = Thread.CurrentThread.CurrentCulture.ToString();
            switch (culture)
            {
                case "nl-NL":
                    Language.Resources.Culture = new CultureInfo("nl-NL");
                    LangResource = new ResXResourceSet(LanguagePath + @"\Resource.nl-NL.resx");
                    break;
                case "en-NL":
                    Language.Resources.Culture = new CultureInfo("en-NL");
                    LangResource = new ResXResourceSet(LanguagePath + @"\Resources.resx");
                    break;
                case "en-GB":
                    Language.Resources.Culture = new CultureInfo("en-GB");
                    LangResource = new ResXResourceSet(LanguagePath + @"\Resources.resx");
                    break;
                default:
                    Language.Resources.Culture = new CultureInfo("en-GB");
                    LangResource = new ResXResourceSet(LanguagePath + @"\Resources.resx");
                    break;
            }
        }
    }
}
