using Indiceringsmodule.Common;
using Indiceringsmodule.Presentation;
using Indiceringsmodule.WPFViews;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
            Application.Current.MainWindow.Show();
        }

        private static void ComposeObjects()
        {
            var ea = new EventAggregator();
            var menu = new Presentation.Menu(ea);
            var viewModel = new MainWindowViewModel(ea, menu)
            {
                CurrentViewModel = new IndiceringsmoduleTestViewModel(ea)
            };
            Current.MainWindow = new MainWindow(ea, viewModel);
        }
    }
}
