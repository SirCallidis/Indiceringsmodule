using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.DataAccess
{
    public class FileLoader
    {
        #region Fields & Properties

        private static readonly string BackupDirectoryName = "backup";
        private readonly List<Subscription> Subscriptions = new List<Subscription>();
        private readonly EventAggregator Ea;
        private readonly string _path;
        private readonly string _rootPath;
        public List<KeyValuePair<string, string[]>> FactMemberExtraFields { get; private set; }

        #endregion Fields & Properties

        #region Default Constructor

        public FileLoader(EventAggregator ea)
        {
            Ea = ea;
            _path = AppDomain.CurrentDomain.BaseDirectory;
            _rootPath = new DirectoryInfo(_path).Parent.Parent.FullName;
            FactMemberExtraFields = LoadFactMemberExtras();
            Subscriptions.Add(Ea.Subscribe<RequestExtraSetsEventModel>(m => SendingExtraSets()));
        }

        #endregion Default Constructor

        #region Methods

        /// <summary>
        /// opens an OpenFileDialog and publishes events based on which file type was selected.
        /// </summary>
        public void LoadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Microsoft Word Documents (*.docx)|*.docx|Rich Text Documents (*.rtf)|*.rtf|Json files (*.json)|*.json|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 2,
            };

            var targetDirectory = _rootPath + @"\SavedFiles";
            ofd.InitialDirectory = targetDirectory;

            if (ofd.ShowDialog() == true)
            {
                var inputFilePath = ofd.FileName;

                //check if backup directory exists
                var backupFileDirectoryPath = Path.Combine(_rootPath, BackupDirectoryName);
                Directory.CreateDirectory(backupFileDirectoryPath);

                //copy file to backup dir
                var inputFileName = Path.GetFileName(inputFilePath);
                var backupFilePath = Path.Combine(backupFileDirectoryPath, inputFileName);
                File.SetAttributes(inputFilePath, FileAttributes.Normal);
                if (File.Exists(backupFilePath))
                {
                    File.SetAttributes(backupFilePath, FileAttributes.Normal);
                }
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
                            case ".txt":
                                var docOb = LoadTXTIntoDocOb(inputFilePath);
                                //Ea.Publish(new DocObLoadedEventModel() { Data = docOb }); //uncomment this once LoadTXTIntoDocOb is fully functional. also create DocObLoadedEventModel/
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

        /// <summary>
        /// starts an OpenFileDialog for pjg images and returns the loaded image
        /// together with its name as a string.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, BitmapImage> LoadImage()
        {
            var result = new KeyValuePair<string, BitmapImage> ( null, null );

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Jpg (*.jpg)|*.jpg",
            };

            var targetDirectory = _rootPath + @"\SavedFiles";
            ofd.InitialDirectory = targetDirectory;

            if (ofd.ShowDialog() == true)
            {
                var inputFilePath = ofd.FileName;

                //check if backup directory exists
                var backupFileDirectoryPath = Path.Combine(_rootPath + "backup");
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

        /// <summary>
        /// Loads the jpg image using a serviceReader
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        private KeyValuePair<string, BitmapImage> OpenJpg(string inputFilePath)
        {
            using (var reader = new ServiceReaderJPG())
            {
                var imageAndNamePair = reader.LoadBitmapPair(inputFilePath);
                return imageAndNamePair;
            };
        }

        /// <summary>
        /// Creates a new DocOb and loads the file data into it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>the DocOb loaded with the data from file.</returns>
        private DocumentObject LoadTXTIntoDocOb(string path)
        {
            var rawData = File.ReadAllText(path);
            var rawObject = JsonConvert.DeserializeObject<JsonMetaDocOb>(rawData);

            var docOb = new DocumentObject(Ea);
            foreach (var str in rawObject.FactMembers)
            {
                PopulateFactMembers(docOb, str);
            }

            docOb.Settings = rawObject.Settings;
            docOb.TranscriptionDocument = null;
            docOb.TotalFacts = null;

            return docOb;
        }

        /// <summary>
        /// Deserializes the FactMembers and puts them in their designated locations.
        /// </summary>
        /// <param name="docOb"></param>
        /// <param name="str"></param>
        private void PopulateFactMembers(DocumentObject docOb, string str)
        {
            //Split into parts
            var splitParts = str.Split('|');
            var factMemberParts = new string[splitParts.Length];
            for (int i = 0; i < splitParts.Length; i++)
            {
                var trimmedPart = splitParts[i].Trim();
                factMemberParts[i] = trimmedPart;
            }

            //Isolate the ID number
            var num = "";
            var firstPart = factMemberParts[0];
            var sub = firstPart.Substring(3, 2);
            if (char.IsDigit(sub[1]))
            {
                num = firstPart;
            }
            else
            {
                num = sub[0].ToString();
            }
            var id = int.Parse(num);

            //Create Hyperlink
            var link = new Hyperlink(new Run(factMemberParts[1]));

            //Deserialize FactMember
            var newFM = new FactMember();
            switch (factMemberParts[2])
            {
                case "Person":
                    newFM = JsonConvert.DeserializeObject<Person>(factMemberParts[3]);
                    break;
                case "RealEstate":
                    newFM = JsonConvert.DeserializeObject<RealEstate>(factMemberParts[3]);
                    break;
                case "Chattel":
                    newFM = JsonConvert.DeserializeObject<Chattel>(factMemberParts[3]);
                    break;
                default:
                    break;
            }
            newFM.ID = id;
            newFM.Link = link;

            //Put it together
            var newf = new Fact(1, "", Ea); //<= temporary: change this to assign fact in DocOb
            newf.TotalFactMembers.Add(link, newFM);
        }

        /// <summary>
        /// Loads additional sets and their members from an external file on startup.
        /// </summary>
        /// <returns>List of the setnames and their content attribute names</returns>
        public List<KeyValuePair<string, string[]>> LoadFactMemberExtras()
        {
            var targetFile = _rootPath + @"\FactMemberFields.json";
            if (!File.Exists(targetFile))
            {
                File.Create(targetFile);
            }
            var rawData = File.ReadAllText(targetFile);
            try
            {
                var list = new List<KeyValuePair<string, string[]>>();
                list = JsonConvert.DeserializeObject<List<KeyValuePair<string, string[]>>>(rawData);
                return list;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// loads the information about Extra sets from an external file and puts
        /// their names and contents in a list.
        /// </summary>
        /// <param name="targetFile"></param>
        /// <returns>List containing the loaded sets: their names and contents.</returns>
        public List<KeyValuePair<string, string[]>> LoadFactMemberExtras(string targetFile)
        {            
            if (!File.Exists(targetFile))
            {
                File.Create(targetFile);
            }
            var rawData = File.ReadAllText(targetFile);
            try
            {
                var list = new List<KeyValuePair<string, string[]>>();
                list = JsonConvert.DeserializeObject<List<KeyValuePair<string, string[]>>>(rawData);
                return list;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Makes the Extra sets loaded from file available to other classes through
        /// an event.
        /// </summary>
        private void SendingExtraSets()
        {
            Ea.Publish(new SendingExtraSetsEventModel() { Data = FactMemberExtraFields });
        }

        #endregion Methods
    }
}
