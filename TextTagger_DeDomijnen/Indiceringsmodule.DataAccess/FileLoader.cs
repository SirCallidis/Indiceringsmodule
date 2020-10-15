using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.DataAccess
{
    public class FileLoader
    {
        private static readonly string BackupDirectoryName = "backup";
        private readonly EventAggregator Ea;

        public FileLoader(EventAggregator ea)
        {
            Ea = ea;
        }

        public void LoadFile()
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
                                    Ea.Publish(new DocumentLoadedEventModel() { Data = doc });
                                }
                                break;
                            case ".docx":
                                using (var reader = new ServiceReaderDocx())
                                {
                                    var doc = reader.LoadDocument(inputFilePath);
                                    Ea.Publish(new DocumentLoadedEventModel() { Data = doc });
                                };
                                break;
                            case ".jpg":
                                var result = OpenJpg(inputFilePath);
                                Ea.Publish(new LoadedKVPairStringBitmapimageEventModel() { Data = result });
                                break;
                            case ".json":
                                throw new NotImplementedException();
                            case ".txt":
                                throw new NotImplementedException();
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

        public KeyValuePair<string, BitmapImage> LoadImage()
        {
            var result = new KeyValuePair<string, BitmapImage> ( null, null );

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Jpg (*.jpg)|*.jpg",
            };

            var path = AppDomain.CurrentDomain.BaseDirectory;
            var rootPath = new DirectoryInfo(path).Parent.Parent.FullName;
            var targetDirectory = rootPath + @"\SavedFiles";
            ofd.InitialDirectory = targetDirectory;

            if (ofd.ShowDialog() == true)
            {
                var inputFilePath = ofd.FileName;

                //check if backup directory exists
                var backupFileDirectoryPath = Path.Combine(rootPath + "backup");
                Directory.CreateDirectory(backupFileDirectoryPath);

                //copy file to backup dir
                var inputFileName = Path.GetFileName(inputFilePath);
                var backupFilePath = Path.Combine(backupFileDirectoryPath, inputFileName);
                File.Copy(inputFilePath, backupFilePath, true);

                var ext = Path.GetExtension(inputFilePath);
                
                if (File.Exists(inputFilePath))
                {
                    try
                    {
                        switch (ext)
                        {
                            case ".jpg":
                                result = OpenJpg(inputFilePath);
                                return result;
                            default:
                                throw new ArgumentException($"{ext} is an unsupported file type.");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException($"Error loading file. {e}");
                    }
                }
            }
            return result;
        }

        private KeyValuePair<string, BitmapImage> OpenJpg(string inputFilePath)
        {
            using (var reader = new ServiceReaderJPG())
            {
                var imageAndNamePair = reader.LoadBitmapPair(inputFilePath);
                return imageAndNamePair;
            };
        }
    }
}
