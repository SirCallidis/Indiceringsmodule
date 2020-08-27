using Indiceringsmodule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Indiceringsmodule.Presentation
{
    public class MainWindowViewModel : Observable
    {
        private readonly EventAggregator Ea;
        protected private Menu Menu;
        private Observable _CurrentViewModel;

        public Observable CurrentViewModel
        {
            get { return _CurrentViewModel; }
            set { SetProperty(ref _CurrentViewModel, value); }
        }

        public RelayCommand Menu_LoadFile { get; private set; }
        public RelayCommand Menu_SaveFile { get; private set; }
        public RelayCommand Menu_CloseFile { get; private set; }

        public MainWindowViewModel(EventAggregator ea, Menu menu)
        {
            Ea = ea ?? throw new ArgumentNullException(nameof(ea));
            this.Menu = menu;
            WireUpForm();            
        }

        private void WireUpForm()
        {
            Menu_LoadFile = new RelayCommand(Menu.OnLoadFile, Menu.CanLoadFile);
            Menu_SaveFile = new RelayCommand(Menu.OnSaveFile, Menu.CanSaveFile);
            Menu_CloseFile = new RelayCommand(Menu.OnCloseProgram, Menu.CanCloseProgram);
        }
    }
}
