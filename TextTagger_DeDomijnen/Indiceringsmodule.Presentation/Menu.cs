using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.Common.Interfaces;
using Indiceringsmodule.DataAccess;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Indiceringsmodule.Presentation
{
    public class Menu : Observable
    {
        private readonly EventAggregator Ea;
        private readonly FileLoader fileLoader;
        private readonly FileSaver fileSaver;

        public Menu(EventAggregator ea, FileLoader fileLoader, FileSaver fileSaver)
        {
            Ea = ea;
            this.fileLoader = fileLoader;
            this.fileSaver = fileSaver;
        }

        #region Command Methods - Menu_LoadFile

        /// <summary>
        /// Method that may need to check wether the Load File window can be opened.
        /// currently inactive. Returning true by default.
        /// </summary>
        /// <returns></returns>
        public bool CanLoadFile()
        {
            return true;
        }

        /// <summary>
        /// Opens the browser to search for a file to open and calls methods to load
        /// file into program.
        /// </summary>
        public void OnLoadFile()
        {
            if (CanLoadFile())
            {
                fileLoader.LoadFile();
            }
        }

        internal bool CanEditDocSettings()
        {
            return true;
        }

        internal void OnEditDocSettings()
        {
            if (CanEditDocSettings())
            {
                var editDocSettingsVM = new EditDocSettingsViewModel(Ea);
                Ea.Publish(new RequestDocSettingsEventModel() { });
                Ea.Publish(new RequestViewForViewModelEventModel() { Data = editDocSettingsVM });
            }

        }

        #endregion

        #region Command Methods - Menu_SaveFile

        /// <summary>
        /// Method that may need to check whether the Save File window can be opened.
        /// currently inactive. Returning true by default.
        /// </summary>
        /// <returns></returns>
        public bool CanSaveFile()
        {
            return true;
        }

        /// <summary>
        /// Opens the browser to allow saving a file and calls methods to save
        /// data into file.
        /// </summary>
        public void OnSaveFile()
        {
            if (CanSaveFile())
            {
                fileSaver.SaveFile();
            }
        }

        

        #endregion

        #region Command Methods - Menu_CloseProgram

        /// <summary>
        /// Checks wether data has changed and notifies user.
        /// If no changes were made, returns true.
        /// </summary>
        /// <returns></returns>
        public bool CanCloseProgram()
        {
            if (ValuesChanged)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Terminates program.
        /// </summary>
        public void OnCloseProgram()
        {
            if (CanCloseProgram())
            {
                Application.Current.Shutdown();
            }
        }

        #endregion


    }
}
