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
        private static readonly string BackupDirectoryName = "backup";
        private static readonly string InProgressDirectoryName = "processing";
        private static readonly string CompletedDirectoryName = "complete";
        private readonly List<Subscription> Subscriptions = new List<Subscription>();
        private FlowDocument _doc;

        private readonly EventAggregator ea;
        
        public Menu(EventAggregator ea)
        {
            this.ea = ea;
            Subscriptions.Add(ea.Subscribe<PublishDocumentEventModel>(m => DocumentReceived(m.Document)));
        }

        private void DocumentReceived(FlowDocument document)
        {
            _doc = document;
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
                OpenLoadFileDialog();
            }
        }

        /// <summary>
        /// definitive method that is called for loading a file
        /// when earlier checks passed.
        /// </summary>
        private void OpenLoadFileDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {               
                Filter = "Microsoft Word Documents (*.docx)|*.docx|Rich Text Documents (*.rtf)|*.rtf|Json files (*.json)|*.json|All files (*.*)|*.*",
                FilterIndex = 2,
            };

            var path = AppDomain.CurrentDomain.BaseDirectory;
            //var rootPath = Path.GetFullPath(Path.Combine(path, @"..\..\"));
            var rootPath = new DirectoryInfo(path).Parent.Parent.FullName;
            var targetDirectory = rootPath + @"\SavedFiles";
            ofd.InitialDirectory = targetDirectory;

            if (ofd.ShowDialog() == true)
            {
                var inputFilePath = ofd.FileName;

                //check if backup directory exists
                //var inputFileDirectoryPath = Path.GetDirectoryName(inputFilePath);
                var backupFileDirectoryPath = Path.Combine(rootPath, BackupDirectoryName);
                Directory.CreateDirectory(backupFileDirectoryPath);

                //copy file to backup dir
                var inputFileName = Path.GetFileName(inputFilePath);
                var backupFilePath = Path.Combine(backupFileDirectoryPath, inputFileName);
                File.Copy(inputFilePath, backupFilePath, true);

                //move file to in progress dir
                //Directory.CreateDirectory(Path.Combine(rootPath, InProgressDirectoryName));
                //var inProgressFilePath =
                //    Path.Combine(rootPath, InProgressDirectoryName, inputFileName);
                //if (File.Exists(inProgressFilePath))
                //{
                //    throw new ArgumentException($"a file with the name {inProgressFilePath} is already being used");
                //}
                //File.Move(inputFilePath, inProgressFilePath);

                var ext = Path.GetExtension(inputFilePath);
                if (File.Exists(inputFilePath))
                {
                    try
                    {
                        switch (ext)
                        {
                            case ".rtf":
                                using (var reader = new ServiceReaderRTF())
                                {
                                    var doc = reader.LoadDocument(inputFilePath);
                                    ea.Publish(new DocumentLoadedEventModel() { Document = doc });
                                }
                                break;
                            case ".docx":
                                using (var reader = new ServiceReaderDocx())
                                {
                                    var doc = reader.LoadDocument(inputFilePath);
                                    ea.Publish(new DocumentLoadedEventModel() { Document = doc });
                                };
                                break;
                            case ".jpg":
                                using (var reader = new ServiceReaderJPG())
                                {
                                    var doc = reader.LoadDocument(inputFilePath);
                                    ea.Publish(new DocumentLoadedEventModel() { Document = doc });
                                };
                                break;
                            case ".json":
                                break;
                            case ".txt":
                                break;
                            default:
                                throw new ArgumentException($"{ext} is an unsupported file type.");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException($"Error loading file. {e}");
                    }
                }

                //move processed file to completed dir
                //var completedDirectoryPath = Path.Combine(rootPath, CompletedDirectoryName);
                //Directory.CreateDirectory(completedDirectoryPath);
                //var completedFileName =
                //    $"{Path.GetFileNameWithoutExtension(inputFilePath)}-{Guid.NewGuid()}{ext}";
                //var completedFilePath = Path.Combine(completedDirectoryPath, completedFileName);
                //File.Move(inProgressFilePath, completedFilePath);
            }
        }

        #endregion

        #region Command Methods - Menu_SaveFile

        /// <summary>
        /// Method that may need to check wether the Save File window can be opened.
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
                OpenSaveFileDialog();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void OpenSaveFileDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Microsoft Word Documents (*.docx)|*.docx|Rich Text Documents (*.rtf)|*.rtf|Json files (*.json)|*.json",
                DefaultExt = ".json"
            };

            var path = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = Path.GetFullPath(Path.Combine(path, @"..\..\"));
            var targetDirectory = rootPath + @"SavedFiles";

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            saveFileDialog.InitialDirectory = targetDirectory;

            ea.Publish(new RequestDocumentForSavingEventModel() { });

            if (saveFileDialog.ShowDialog() == true)
            {
                //TODO - This happens when file is saved/
                var selectedPath = saveFileDialog.FileName;               
                Task.Run(() => SaveFileToJson(selectedPath));
                ValuesChanged = false;
            }
            else
            {
                MessageBox.Show("Error while saving");
            }
        }

        private void SaveFileToJson(string path)
        {
            var reader = new ServiceReaderJSON();
            reader.SaveDocument(_doc, path);
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
