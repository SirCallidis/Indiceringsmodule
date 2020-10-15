using Indiceringsmodule.Common;
using Indiceringsmodule.Presentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Indiceringsmodule.WPFViews
{
    public partial class MainWindow : Window
    {
        private readonly EventAggregator Ea;
        private readonly MainWindowViewModel ViewModel;

        public MainWindow(EventAggregator ea, MainWindowViewModel viewModel)
        {
            InitializeComponent();
            Ea = ea;
            ViewModel = viewModel;
            DataContext = ViewModel;
        }
    }
}
