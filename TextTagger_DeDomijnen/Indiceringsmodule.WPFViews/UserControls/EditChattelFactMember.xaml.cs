using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
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
    /// Interaction logic for EditChattelFactMember.xaml
    /// </summary>
    public partial class EditChattelFactMember : UserControl
    {
        #region Fields

        private EventAggregator Ea;
        private readonly Label[] _ExtraLabels;
        private readonly TextBox[] _ExtraTBs;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();

        #endregion Fields

        #region Constructor

        public EditChattelFactMember(EventAggregator ea)
        {           
            InitializeComponent();
            Ea = ea;
            Subscriptions.Add(Ea.Subscribe<DisplaySetContentEventModel>(m => DisplaySetContent(m.Data)));
            _ExtraLabels = new Label[15];
            _ExtraTBs = new TextBox[15];
            InitializeExtraArrays();
        }

        #endregion Constructor

        #region Methods 

        /// <summary>
        /// Assigns the Extra Properties labels and textboxes to arrays
        /// (nondynamically, may eventually refactor if more fields are needed)
        /// </summary>
        private void InitializeExtraArrays()
        {
            _ExtraLabels[0] = Prop0Label;
            _ExtraLabels[1] = Prop1Label;
            _ExtraLabels[2] = Prop2Label;
            _ExtraLabels[3] = Prop3Label;
            _ExtraLabels[4] = Prop4Label;
            _ExtraLabels[5] = Prop5Label;
            _ExtraLabels[6] = Prop6Label;
            _ExtraLabels[7] = Prop7Label;
            _ExtraLabels[8] = Prop8Label;
            _ExtraLabels[9] = Prop9Label;
            _ExtraLabels[10] = Prop10Label;
            _ExtraLabels[11] = Prop11Label;
            _ExtraLabels[12] = Prop12Label;
            _ExtraLabels[13] = Prop13Label;
            _ExtraLabels[14] = Prop14Label;
            _ExtraTBs[0] = Prop0TB;
            _ExtraTBs[1] = Prop1TB;
            _ExtraTBs[2] = Prop2TB;
            _ExtraTBs[3] = Prop3TB;
            _ExtraTBs[4] = Prop4TB;
            _ExtraTBs[5] = Prop5TB;
            _ExtraTBs[6] = Prop6TB;
            _ExtraTBs[7] = Prop7TB;
            _ExtraTBs[8] = Prop8TB;
            _ExtraTBs[9] = Prop9TB;
            _ExtraTBs[10] = Prop10TB;
            _ExtraTBs[11] = Prop11TB;
            _ExtraTBs[12] = Prop12TB;
            _ExtraTBs[13] = Prop13TB;
            _ExtraTBs[14] = Prop14TB;
        }

        /// <summary>
        /// Enables Textboxes and Labels in the view and renames the Labels based on
        /// incoming parameter "data".
        /// </summary>
        /// <param name="data">The strings that form the content of the enabled labels</param>
        private void DisplaySetContent(string[] data)
        {
            ResetSetDisplays();
            for (int i = 0; i < data.Length; i++)
            {
                _ExtraLabels[i].IsEnabled = true;
                _ExtraLabels[i].Visibility = Visibility.Visible;
                _ExtraLabels[i].Content = data[i];
                _ExtraTBs[i].IsEnabled = true;
                _ExtraTBs[i].Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Clears, collapses and disables each Label and Textbox in _ExtraLabels and _ExtraTBs.
        /// </summary>
        private void ResetSetDisplays()
        {
            for (int i = 0; i < _ExtraLabels.Length; i++)
            {
                _ExtraLabels[i].Content = string.Empty;
                _ExtraLabels[i].Visibility = Visibility.Collapsed;
                _ExtraLabels[i].IsEnabled = false;
                _ExtraTBs[i].Visibility = Visibility.Collapsed;
                _ExtraTBs[i].IsEnabled = false;                
            }
        }

        #endregion Methods
    }
}
