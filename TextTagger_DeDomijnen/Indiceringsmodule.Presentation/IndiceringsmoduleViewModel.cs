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
        private Fact _SelectedFact;
        public Fact SelectedFact
        {
            get { return _SelectedFact; }
            set
            {
                SetProperty(ref _SelectedFact, value);
                ChangeSelectedFactNumber(value.ID);
                OnSelectedFactChanged(value);
            }
        }

        //the number displayed in the UI representing the currently selected
        //fact
        private int _SelectedFactNumber;
        public int SelectedFactNumber
        {
            get { return _SelectedFactNumber; }
            set { SetProperty(ref _SelectedFactNumber, value); }
        }

        //the currently selected FactMember
        private string _SelectedFactMember;
        public string SelectedFactMember
        {
            get { return _SelectedFactMember; }
            set { SetProperty(ref _SelectedFactMember, value); }
        }

        //list of possible FactMember type names which can be selected
        private List<string> _FactMembers;
        public List<string> FactMembers
        {
            get { return _FactMembers; }
            set { SetProperty(ref _FactMembers, value); }
        }

        //the name + extension of the currently selected (and displayed) image
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

        //the currently selected Image
        private BitmapImage _SelectedImage;
        public BitmapImage SelectedImage
        {
            get { return _SelectedImage; }
            set{ SetProperty(ref _SelectedImage, value); }
        }

        //List if names from each image
        private ObservableCollection<string> _ImageNameList;
        public ObservableCollection<string> ImageNameList
        {
            get { return _ImageNameList; }
            set { SetProperty(ref _ImageNameList, value); }
        }
        #endregion

        #region Relay Commands

        public RelayCommand Button_CreateFactMember { get; private set; }
        public RelayCommand Button_AddImage { get; private set; }
        public RelayCommand Button_RemoveImage { get; private set; }
        public RelayCommand Button_SelectNextFact { get; private set; }
        public RelayCommand Button_SelectPreviousFact { get; private set; }

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
            Subscriptions.Add(Ea.Subscribe<CreateFactEventModel>(m => CreateFact(m.Data)));
            Subscriptions.Add(Ea.Subscribe<SelectedFactChangedEventModel>(m => SelectedFactChanged(m.Data, m.Direction)));
            Subscriptions.Add(Ea.Subscribe<SaveCurrentFactDocumentToFact>(m => SetInboundFactDocumentToFact(m.Data)));           

            Button_CreateFactMember = new RelayCommand(OnCreateFactMember, CanCreateFactMember);
            Button_AddImage = new RelayCommand(OnAddImage, CanAddImage);
            Button_RemoveImage = new RelayCommand(OnRemoveImage, CanRemoveImage);
            Button_SelectNextFact = new RelayCommand(OnSelectNextFact, CanSelectNextFact);
            Button_SelectPreviousFact = new RelayCommand(OnSelectPreviousFact, CanSelectPreviousFact);

            DocumentObject = new DocumentObject();

            //TODO - fix hardcoded list content
            FactMembers = new List<string>() { "*Person", "*RealEstate", "*Chattel" };
            ImageNameList = new ObservableCollection<string>() { "*<No Image Selected>" };
            
            SelectedImageName = ImageNameList[0];
        }

        #endregion

        #region General Methods

        /// <summary>
        /// Publishes an event that makes the instance of the Settings class
        /// of the DocumentObject available to the subscriber.
        /// </summary>
        private void MakeDocSettingsAvailable()
        {
            Ea.Publish(new PublishDocSettingsEventModel() { Data = DocumentObject.Settings });
        }

        /// <summary>
        /// Disposes of the object's subscriptions on AE
        /// when called.
        /// </summary>
        public void Dispose()
        {
            foreach (var sub in Subscriptions)
            {
                sub?.Dispose();
            }
        }

        #endregion

        #region Methods dealing with Adding / Removing / Displaying (Jpg) Images

        /// <summary>
        /// Validates if (an) Image can be removed based on
        /// the current SelectedImageName
        /// </summary>
        /// <returns></returns>
        private bool CanRemoveImage()
        {
            if (SelectedImageName != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fires an event to update the View
        /// and removes the Image from the DocumentObject's collection
        /// as well as the ImageName from this.ImageNameList.
        /// It then sets the selected image and name to default.
        /// </summary>
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
                DocumentObject.Images.TryGetValue(SelectedImageName, out BitmapImage image);
                SelectedImage = image;
            }
            if(SelectedImageName == null)
            {
                SelectedImage = null;
            }
        }

        #endregion

        #region Methods dealing Adding / Removing / Selecting Facts and FactMembers

        /// <summary>
        /// Saves the FlowDocument data to the currently selected fact's
        /// FactDocument, then calls methods to find either the next or previous
        /// Fact
        /// </summary>
        /// <param name="data"></param>
        /// <param name="direction"></param>
        private void SelectedFactChanged(FlowDocument data, Enums.direction direction)
        {
            if (SelectedFact != null)
            {
                SelectedFact.FactDocument = data;
                if (direction == Enums.direction.next)
                {
                    OnSelectNextFact();
                }
                if (direction == Enums.direction.previous)
                {
                    OnSelectPreviousFact();
                }
            }
        }

        /// <summary>
        /// Returns a boolean based on whether a previous fact,
        /// meaning one with a lower ID number, can be selected.
        /// </summary>
        /// <returns></returns>
        private bool CanSelectPreviousFact()
        {
            if (SelectedFact == null) return false;
            var currentIDnumber = SelectedFact.ID;
            var result = DocumentObject.IsThereALowerFactID(currentIDnumber);
            if (result)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the previous fact in the list and sets it as the SelectedFact
        /// </summary>
        private void OnSelectPreviousFact()
        {
            if (CanSelectPreviousFact())
            {
                var previousFactID = SelectedFact.ID - 1;
                var fact = DocumentObject.TotalFacts.Where(f => f.ID == previousFactID).First();
                SelectedFact = fact;
            }
        }

        /// <summary>
        /// Returns a boolean based on whether a next fact,
        /// meaning one with a higher ID number, can be selected.
        /// </summary>
        /// <returns></returns>
        private bool CanSelectNextFact()
        {
            if (SelectedFact == null) return false;
            var currentIDnumber = SelectedFact.ID;
            var result = DocumentObject.IsThereAHigherFactID(currentIDnumber);
            if (result)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the next fact in the list and sets it as the SelectedFact
        /// </summary>
        private void OnSelectNextFact()
        {
            if (CanSelectNextFact())
            {
                var nextFactID = SelectedFact.ID + 1;
                var fact = DocumentObject.TotalFacts.Where(f => f.ID == nextFactID).First();
                SelectedFact = fact;
            }
        }

        /// <summary>
        /// Fires an event with the fact as a payload to notify the view
        /// to update its appropriate ui element.
        /// </summary>
        /// <param name="factDocument">FlowDocument payload of the event.</param>
        private void OnSelectedFactChanged(Fact fact)
        {
            Ea.Publish(new PublishFactEventModel() { Data = fact });
        }

        /// <summary>
        /// Creates a new Fact and sets the CurrentFact to a reference of it
        /// </summary>
        private void CreateFact(string selection)
        {
            DocumentObject.CreateFact(selection);
            var newlyCreatedFact = DocumentObject.TotalFacts.OrderByDescending(f => f.ID).First();
            Ea.Publish(new NewFactWasCreatedEventModel() { Data = newlyCreatedFact });
            SelectedFact = newlyCreatedFact;
        }

        /// <summary>
        /// Sets the SelectedFactNumber to display correctly.
        /// basically parsing Fact ID's 0-based int to the list's 1-based int. 
        /// </summary>
        /// <param name="id"></param>
        private void ChangeSelectedFactNumber(int id)
        {
            SelectedFactNumber = id + 1;
        }

        /// <summary>
        /// Receives a flowDocument and sets it as the 
        /// selectedFact's FactDocument property.
        /// </summary>
        /// <param name="doc"></param>
        private void SetInboundFactDocumentToFact(FlowDocument doc)
        {
            SelectedFact.FactDocument = doc;
        }

        /// <summary>
        /// Checks whether a new Fact Member can be created.
        /// Always returns true, may be a place holder for future logic.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateFactMember()
        {
            //validate selection?
            return true;
        }

        private void OnCreateFactMember()
        {
            //is fired by view codebehind
            if (CanCreateFactMember())
            {
                //event that asks for what the current selection is

                //takes richTB.Selection from View
                //checks if SelectedThing != null

                string selection = "".Trim();

                switch (SelectedFactMember)
                {
                    case "*Person":
                        SelectedFact.CreatePerson(selection);
                        break;
                    case "*RealEstate":
                        SelectedFact.CreateRealEstate(selection);
                        break;
                    case "*Chattel":
                        SelectedFact.CreateChattel(selection);
                        break;
                    case null:
                        MessageBox.Show("*Select a fact type first");
                        break;
                    default:
                        throw new ArgumentException("Could not process input:" + SelectedFactMember);
                }
            }
        }
        #endregion




        private void MakeDocumentAvailable()
        {
            //TODO - Needs work: this needs to return the whole DocObject
            Ea.Publish(new PublishDocumentEventModel() { Data = DocumentObject });
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
    }
}
