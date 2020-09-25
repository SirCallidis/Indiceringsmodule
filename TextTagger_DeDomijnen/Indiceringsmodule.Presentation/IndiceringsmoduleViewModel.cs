using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.DataAccess;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Indiceringsmodule.Presentation
{
    public class IndiceringsmoduleViewModel : Observable, IDisposable
    {
        #region Fields & Properties

        public readonly EventAggregator Ea;
        private readonly FileLoader fileLoader;
        private readonly FileSaver fileSaver;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();


        //the top-level data structure that holds documents, images, facts, factmembers etc.
        private DocumentObject _DocumentObject;
        public DocumentObject DocumentObject
        {
            get { return _DocumentObject; }
            set { SetProperty(ref _DocumentObject, value); }
        }

        //holds a reference to the currently selected Fact in the view
        private Fact _CurrentFact;
        public Fact CurrentFact
        {
            get { return _CurrentFact; }
            set { SetProperty(ref _CurrentFact, value); }
        }

        private string _SelectedFactMember;
        public string SelectedFactMember
        {
            get { return _SelectedFactMember; }
            set { SetProperty(ref _SelectedFactMember, value); }
        }

        private List<string> _FactMembers;
        public List<string> FactMembers
        {
            get { return _FactMembers; }
            set { SetProperty(ref _FactMembers, value); }
        }

        private UserControl _FactMemberView;
        public UserControl FactMemberView
        {
            get { return _FactMemberView; }
            set { SetProperty(ref _FactMemberView, value); }
        }

        private string _SelectedImageName;
        public string SelectedImageName
        {
            get { return _SelectedImageName; }
            set 
            {
                SetProperty(ref _SelectedImageName, value);
                SelectedImageNameChanged(value);
            }
        }

        private BitmapImage _SelectedImage;
        public BitmapImage SelectedImage
        {
            get { return _SelectedImage; }
            set{ SetProperty(ref _SelectedImage, value); }
        }

        private ObservableCollection<string> _ImageNameList;
        public ObservableCollection<string> ImageNameList
        {
            get { return _ImageNameList; }
            set { SetProperty(ref _ImageNameList, value); }
        }
        #endregion


        #region Relay Commands

        public RelayCommand Button_CreateFact { get; private set; }
        public RelayCommand Button_CreateFactMember { get; private set; }
        public RelayCommand Button_AddImage { get; private set; }
        public RelayCommand Button_RemoveImage { get; private set; }

        #endregion

        #region Default Constructor

        public IndiceringsmoduleViewModel(EventAggregator ea, FileLoader fileLoader, FileSaver fileSaver)
        {
            Ea = ea;
            this.fileLoader = fileLoader;
            this.fileSaver = fileSaver;
            WireUpForm();
        }

        private void WireUpForm()
        {
            //Subscriptions.Add(Ea.Subscribe<DocumentLoadedEventModel>(m => DisplayNewCurrentDocument(m.Document)));
            Subscriptions.Add(Ea.Subscribe<RequestDocumentForSavingEventModel>(m => MakeDocumentAvailable()));
            Subscriptions.Add(Ea.Subscribe<RequestDocSettingsEventModel>(m => MakeDocSettingsAvailable()));
            Subscriptions.Add(Ea.Subscribe<LoadedKVPairStringBitmapimageEventModel>(m => CompareLoadedImage(m.Data)));         

            Button_CreateFact = new RelayCommand(OnCreateFact, CanCreateFact);
            Button_CreateFactMember = new RelayCommand(OnCreateFactMember, CanCreateFactMember);
            Button_AddImage = new RelayCommand(OnAddImage, CanAddImage);
            Button_RemoveImage = new RelayCommand(OnRemoveImage, CanRemoveImage);

            DocumentObject = new DocumentObject();

            //TODO - fix hardcoded list content
            FactMembers = new List<string>() { "*Person", "*RealEstate", "*Chattel" };
            ImageNameList = new ObservableCollection<string>() { "*<No Image Selected>" };
            SelectedImageName = ImageNameList[0];
        }

        #endregion

        /// <summary>
        /// Checks wether the CurrentDocument property is null,
        /// if it is, it sets the document parameter as the new CurrentDocument.
        /// If it isn't, it'll append the document parameter to the existing
        /// CurrentDocument.
        /// </summary>
        /// <param name="document"></param>
        //private void DisplayNewCurrentDocument(FlowDocument document)
        //{
        //    if (ImageFlowDocument == null)
        //    {
        //        ImageFlowDocument = document;
        //    }
        //    else
        //    {
        //        for (int i = 0; i < document.Blocks.Count; i++)
        //        {
        //            ImageFlowDocument.Blocks.Add(document.Blocks.ElementAt(i));
        //        }
        //    }           
        //}

        #region Methods dealing with Adding / Removing / Displaying (Jpg) Images

        private bool CanRemoveImage()
        {
            if (SelectedImageName != null)
            {
                return true;
            }
            return false;
        }

        private void OnRemoveImage()
        {
            if (CanRemoveImage())
            {               
                if (SelectedImageName != ImageNameList[0])
                {
                    Ea.Publish(new UpdateViewEventModel() { Data = SelectedImageName });
                    DocumentObject.Images.Remove(SelectedImageName);
                    ImageNameList.Remove(SelectedImageName);
                    SelectedImageName = ImageNameList[0];
                    SelectedImage = null;
                }                
            }  
        }

        /// <summary>
        /// Placeholder for possible additional validation logic.
        /// Returns true by default.
        /// </summary>
        /// <returns></returns>
        private bool CanAddImage()
        {
            return true;
        }

        /// <summary>
        /// Checks whether LoadImage may be called.
        /// </summary>
        private void OnAddImage()
        {
            if (CanAddImage())
            {
                LoadImage();
            }
        }

        /// <summary>
        /// Opens a new instance of OpenFileDialog, accepting only Jpg files.
        /// Attempts to load the Jpg into BitmapImage and binds it with a key
        /// in a KV-Pair.
        /// Then checks whether current Key is already contained in DocumentObject's
        /// Images list. If not, it'll add it. If so, shows an notification.
        /// </summary>
        public void LoadImage()
        {
            var result = fileLoader.LoadImage();
            CompareLoadedImage(result);            
        }

        /// <summary>
        /// Checks whether current Key is already contained in DocumentObject's
        /// Images list. If not, it'll add it. If so, shows an notification.
        /// </summary>
        /// <param name="result"></param>
        private void CompareLoadedImage(KeyValuePair<string, BitmapImage> result)
        {
            var keyExists = DocumentObject.Images.Keys.Where(k => k.Contains(result.Key)).Any();
            if (!keyExists)
            {
                DocumentObject.Images.Add(result.Key, result.Value);
                ImageNameList.Add(result.Key);
                Ea.Publish(new UpdateViewEventModel() { Data = result.Key });
                SelectedImage = result.Value;
                SelectedImageName = result.Key;
            }
            else
            {
                MessageBox.Show("*A file of that name already exists");
            }
        }

        /// <summary>
        /// Sets the Image to be displayed in the View
        /// based off of the SelectedImageName
        /// </summary>
        /// <param name="SelectedImageName"></param>
        private void SelectedImageNameChanged(string SelectedImageName)
        {
            if (SelectedImageName != null)
            {
                BitmapImage image;
                DocumentObject.Images.TryGetValue(SelectedImageName, out image);
                SelectedImage = image;
            }
            if(SelectedImageName == null)
            {
                SelectedImage = null;
            }
        }

        #endregion

        /// <summary>
        /// Checks wether a new Fact can be created.
        /// Always returns true, may be a place holder for future logic.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateFact()
        {
            return true;
        }

        /// <summary>
        /// Creates a new Fact and sets the CurrentFact to a reference of it
        /// </summary>
        private void OnCreateFact()
        {
            if (CanCreateFact())
            {
                DocumentObject.CreateFact();
                CurrentFact = DocumentObject.TotalFacts.OrderByDescending(f => f.ID).First();
            }
        }

        /// <summary>
        /// Checks wether a new Fact Member can be created.
        /// Always returns true, may be a place holder for future logic.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateFactMember()
        {
            return true;
        }

        private void OnCreateFactMember()
        {
            if (CanCreateFactMember())
            {
                //event that asks for what the current selection is

                //takes richTB.Selection from View
                //checks if SelectedThing != null

                string selection = "".Trim();

                switch (SelectedFactMember)
                {
                    case "*Person":
                        CurrentFact.CreatePerson(selection);
                        break;
                    case "*RealEstate":
                        CurrentFact.CreateRealEstate(selection);
                        break;
                    case "*Chattel":
                        CurrentFact.CreateChattel(selection);
                        break;                        
                    default:
                        throw new ArgumentException("Could not process input:" + SelectedFactMember);
                }
            }
        }


        private void MakeDocumentAvailable()
        {
            //TODO - Needs work: this needs to return the whole DocObject
            Ea.Publish(new PublishDocumentEventModel() { Data = DocumentObject });
        }

        /// <summary>
        /// Publishes an event that makes the instance of the Settings class
        /// of the DocumentObject available to the subscriber.
        /// </summary>
        private void MakeDocSettingsAvailable()
        {
            Ea.Publish(new PublishDocSettingsEventModel() { Data = DocumentObject.Settings });
        }


        private IEnumerable<Block> RemoveChildren(Block block)
        {
            var list = new List<Block>();
            if (block is BlockUIContainer)
            {
                var b = block as BlockUIContainer;
                b.Child = null;
                list.Add(b);
                return list;
            }
            return list;
        }

        public void Dispose()
        {
            foreach (var sub in Subscriptions)
            {
                sub?.Dispose();
            }
        }
    }
}
