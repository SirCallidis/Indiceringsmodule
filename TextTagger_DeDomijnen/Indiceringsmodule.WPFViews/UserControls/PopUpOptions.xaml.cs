using Indiceringsmodule.Common;
using Indiceringsmodule.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Indiceringsmodule.WPFViews.UserControls
{
    /// <summary>
    /// Interaction logic for PopUpOptions.xaml
    /// </summary>
    public partial class PopUpOptions : UserControl
    {
        private protected EventAggregator Ea;

        public PopUpOptions(EventAggregator ea)
        {
            InitializeComponent();
            Ea = ea;
        }
    }
}
