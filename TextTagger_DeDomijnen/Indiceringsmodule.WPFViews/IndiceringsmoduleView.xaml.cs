using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.DataAccess;
using Indiceringsmodule.Presentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

namespace Indiceringsmodule.WPFViews
{
    /// <summary>
    /// Interaction logic for IndiceringsmoduleRoot.xaml
    /// </summary>
    public partial class IndiceringsmoduleView : UserControl
    {

        #region Fields & Properties
            
        private protected EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();
        private double oldSize;
        private Dictionary<int, RichTextBox> FactDocumentUIs;
        public RichTextBox CurrentFactRTB;

        #endregion

        #region Default Constructor

        public IndiceringsmoduleView(EventAggregator ea)
        {
            Ea = ea;
            InitializeComponent();
            DataObject.AddPastingHandler(this, OnPaste);
            SetRichTBTextSettings(transcriptionRichTB.Document);
            CurrentFactRTB = CreateNewFactRTB();
            SetRichTBTextSettings(CurrentFactRTB.Document);            
            ResetSlider();           

            Subscriptions.Add(Ea.Subscribe<UpdateViewEventModel>(m => UpdateView(m.Data)));
            Subscriptions.Add(Ea.Subscribe<DocumentLoadedEventModel>(m => LoadedFlowDocumentReceived(m.Data)));
            Subscriptions.Add(Ea.Subscribe<PublishFactEventModel>(m => UpdateUIOnFactReceived(m.Data)));
            Subscriptions.Add(Ea.Subscribe<NewFactWasCreatedEventModel>(m => OnNewFactCreated(m.Data)));

            FactDocumentUIs = new Dictionary<int, RichTextBox>();
        }

        #endregion

        #region General Helper Methods

        /// <summary>
        /// Sets the slider halfway, so a positive and negative zoom can be achieved.
        /// </summary>
        private void ResetSlider()
        {
            slider.Value = 0.55;
        }

        /// <summary>
        /// sets the Font Family, Size, and Weight of the FlowDocument
        /// to (for now) hardcoded values.
        /// </summary>
        private void SetRichTBTextSettings(FlowDocument doc)
        {
            if (doc == null) throw new ArgumentNullException("Document cannot be null!");
            doc.FontWeight = FontWeights.Normal;
            doc.FontSize = 16;
            doc.FontFamily = new FontFamily("Segoe");
        }

        /// <summary>
        /// Takes the text as strings from each Block in the
        /// FlowDocument, then concatonates these to a single string
        /// while saving line endings.
        /// Then wipes the Blocks and creates a new, single
        /// Block and Paragraph with the new large string in it.
        /// </summary>
        /// <param name="doc"></param>
        private void PutAllTextInOneBlock(FlowDocument doc)
        {
            if (doc == null) throw new ArgumentNullException("Document cannot be null!");

            var blockCount = doc.Blocks.Count();

            var strings = new string[blockCount];

            var i = 0;
            foreach (var block in doc.Blocks)
            {
                var range = new TextRange(block.ContentStart, block.ContentEnd);
                var text = range.Text;
                var line = text + "\r\n";
                strings[i] = line;
                i++;
            }
            var totalString = string.Concat(strings);
            doc.Blocks.Clear();
            doc.Blocks.Add(new Paragraph(new Run(totalString)));
        }

        /// <summary>
        /// Method that processes the FlowDocument originating
        /// from a loaded file.
        /// </summary>
        /// <param name="data"></param>
        private void LoadedFlowDocumentReceived(FlowDocument data)
        {
            transcriptionRichTB.Document.Blocks.AddRange(data.Blocks.ToList());
            PutAllTextInOneBlock(transcriptionRichTB.Document);
            SetRichTBTextSettings(transcriptionRichTB.Document);
        }

        /// <summary>
        /// Logic that fires an event signaling the loading
        /// of the next Fact in the list, if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextFactButton_Click(object sender, RoutedEventArgs e)
        {
            Ea.Publish(new SelectedFactChangedEventModel()
            {
                Data = transcriptionRichTB.Document,
                Direction = Enums.direction.next,
            });
        }

        /// <summary>
        /// Logic that fires an event signaling the loading
        /// of the previous Fact in the list, if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousFactButton_Click(object sender, RoutedEventArgs e)
        {
            Ea.Publish(new SelectedFactChangedEventModel()
            {
                Data = transcriptionRichTB.Document,
                Direction = Enums.direction.previous,
            });
        }

        #endregion

        #region Methods dealing with Images

        /// <summary>
        /// Handles the logic of image scaling based on the
        /// sender's input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (image.Source == null) return;

            var path = image.Source.ToString();
            var size = slider.Value;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            double newWidth;
            double newHeight;

            if (size >= oldSize)
            {
                newWidth = image.ActualWidth + (image.ActualWidth * size / 100);
                newHeight = image.ActualHeight + (image.ActualHeight * size / 100);
            }
            else if (size < oldSize)
            {
                newWidth = image.ActualWidth - (image.ActualWidth * size / 100);
                newHeight = image.ActualHeight - (image.ActualHeight * size / 100);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Could not parse slider value: " + size);
            }

            image.Width = newWidth;
            image.Height = newHeight;
            image.Source = bitmap;
            oldSize = size;
        }

        /// <summary>
        /// Displays the image on screen from the ViewModel
        /// (in a tightly coupled, non-MVVM way ...for now)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = this.DataContext as IndiceringsmoduleViewModel;
            image.Source = context.SelectedImage;
        }

        /// <summary>
        /// Updates the ImageSelector's SelectedItem
        /// with the passed variable
        /// </summary>
        /// <param name="docOb"></param>
        private void UpdateView(string value)
        {
            var list = ImageSelector.ItemsSource as ObservableCollection<string>;
            try
            {
                var newlyAddedItemName = list.First(n => n.Equals(value));
                ImageSelector.SelectedItem = newlyAddedItemName;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not find name in list {e}");
            }
        }

        #endregion

        #region Methods dealing with RichTextbox(es)

        /// <summary>
        /// Makes sure only the pasted text as string is
        /// filtered to be pasted into richtextbox.
        /// This omits all textformatting data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (transcriptionRichTB.IsFocused == true)
            {
                string pastingText = e.DataObject.GetData(DataFormats.Text) as string;
                transcriptionRichTB.Document.ContentEnd.InsertTextInRun(pastingText);
                e.CancelCommand();
            }
            if (CurrentFactRTB.IsFocused == true)
            {
                string pastingText = e.DataObject.GetData(DataFormats.Text) as string;
                CurrentFactRTB.Document.ContentEnd.InsertTextInRun(pastingText);
                e.CancelCommand();
            }
            else
            {
                transcriptionRichTB.Focus();
            }

        }

        /// <summary>
        /// Logic that enables or disables the CreateObject button
        /// based on whether text is selected in the richtextbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FactRichTB_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selection = CurrentFactRTB.Selection;
            if (selection.IsEmpty == false)
            {
                CreateFactMemberButton.IsEnabled = true;
            }
            else
            {
                CreateFactMemberButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// sets the factRichTB UI element to display the factDocument variable
        /// on screen.
        /// </summary>
        /// <param name="factDocument"></param>
        private void UpdateUIOnFactReceived(Fact fact)
        {
            RTBDisplayer.Content = FactDocumentUIs.Where(x => x.Key == fact.ID).Select(x => x.Value).First();
        }

        /// <summary>
        /// Creates a new RichTextBox ui control and binds
        /// it to the incoming fact's document.
        /// It adds the newly created RichTextBox to the
        /// FactDocumentsUIs list and sets the displayed
        /// result to this new RTB.
        /// </summary>
        /// <param name="fact"></param>
        private void OnNewFactCreated(Fact fact)
        {
            var rtb = CreateNewFactRTB();
            rtb.Document = fact.FactDocument;

            FactDocumentUIs.Add(fact.ID, rtb);
            
            CurrentFactRTB = FactDocumentUIs.Where(x => x.Key == fact.ID).Select(x => x.Value).First();
            RTBDisplayer.Content = CurrentFactRTB;
        }

        /// <summary>
        /// Creates a new RichTextBox with
        /// hardcoded settings
        /// </summary>
        /// <returns></returns>
        private RichTextBox CreateNewFactRTB()
        {
            var rtb = new RichTextBox
            {
                IsEnabled = true,
                Padding = new Thickness(5),
            };
            rtb.SelectionChanged += FactRichTB_SelectionChanged;
            //TODO: look at if RTB style is needed
            return rtb;
        }

        /// <summary>
        /// Enables and disables the Create Fact Button based on
        /// the transcriptionRichTB selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TranscriptionRichTB_SelectionChanged(object sender, RoutedEventArgs e)
        { 
            var selection = transcriptionRichTB.Selection;
            if (selection.IsEmpty == false)
            {
                CreateFactButton.IsEnabled = true;
            }
            else
            {
                CreateFactButton.IsEnabled = false;
            }
        }

        #endregion

        #region Methods dealing with Fact and FactMember creation

        /// <summary>
        /// Delegates Fact creation validation and signals
        /// Fact creation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateFactButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanCreateFact())
            {
                SignalCreateFact();
            }
        }

        /// <summary>
        /// Returns a boolean based on the validity of the
        /// RichTextBox's selection.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateFact()
        {
            var selection = transcriptionRichTB.Selection.Text;
            var isSelectionNullOrEmpty = string.IsNullOrEmpty(selection);
            if (isSelectionNullOrEmpty)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Fires an event signaling the creation of a new Fact
        /// with the current RTB selection as a parameter.
        /// (this text is used to populate the Fact's FactDocument)
        /// </summary>
        private void SignalCreateFact()
        {
            //if (TotalFactsLabel.Text != "0") //probably not needed as diff. RTB's are stored in memory
            //{                                //if that is the case: also remove followup from VM. 
            //    Ea.Publish(new SaveCurrentFactDocumentToFact() { Data = CurrentFactRTB.Document });
            //}
            var selection = transcriptionRichTB.Selection.Text;
            Ea.Publish(new CreateFactEventModel() { Data = selection });
        }

        /// <summary>
        /// Delegates FactMember creation validation and signals
        /// FactMember creation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateFactMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanCreateFactMember())
            {
                SignalCreateFactMember();
            }
        }

        /// <summary>
        /// Returns a boolean based on the validity of the
        /// RichTextBox's selection.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateFactMember()
        {
            var selection = CurrentFactRTB.Selection.Text;
            if (selection.Length < 1) throw new ArgumentOutOfRangeException($"*Selection is too short. {selection}");
            if (selection.Length > 50) throw new ArgumentOutOfRangeException($"*Selection is too long. {selection}");
            
            var isSelectionNullOrEmpty = string.IsNullOrEmpty(selection);
            if (isSelectionNullOrEmpty)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SignalCreateFactMember()
        {
            var factMemberCreationData = new FactMemberCreationData
            {
                CurrentFact = CurrentFactRTB.Document,
                Selection = CurrentFactRTB.Selection.Text,
                ChosenType = FactMemberSelector.SelectedItem.ToString(),
            };
            //TODO this method may already need to parse the data.
            //What if Selection appears multiple times in document text?
            Ea.Publish(new CreateFactMemberEventModel() { Data = factMemberCreationData});
            
            //on VM:
            //cut sentence in 3 parts on selection
            //make a hlink out of selection(middle part)
            //create new FactMember of chosen type with key: selection
        }

        #endregion



        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



        #region Temporary and Testing Methods

        /// <summary>
        /// method for testing purposes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = CurrentFactRTB.Selection.Text;

            var doc = CurrentFactRTB.Document;
            var para = new Paragraph(new Run("newly added paragraph text!") { FontSize=12 });
            doc.Blocks.Add(para);

            //https://stackoverflow.com/questions/762271/clicking-hyperlinks-in-a-richtextbox-without-holding-down-ctrl-wpf

            //example of using hyperlinks mid text
            Paragraph par = new Paragraph();
            Run run1 = new Run("Text preceeding the hyperlink.");
            Run run2 = new Run("Text following the hyperlink.");
            Run run3 = new Run("Link Text.");

            Hyperlink hLink = new Hyperlink(run3);
            hLink.Click += HLink_Click;
            //for actual web hyperlinks:
            //hLink.RequestNavigate += HLink_RequestNavigate;
            //hLink.NavigateUri = new Uri("http://search.msn.com");

            par.Inlines.Add(run1);
            par.Inlines.Add(hLink);
            par.Inlines.Add(run2);

            doc.Blocks.Add(par);

            


            //var reader = new ServiceReaderJSON();
            //reader.SaveDocument(doc, "invalid");
        }


        /// <summary>
        /// Standard logic for a paste action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaste_Default(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                var composition = new TextComposition(InputManager.Current, this, text);
                TextCompositionManager.StartComposition(composition);
            }
            //should the above need to be canceled, use e.CancelCommand
            //e.CancelCommand();
        }

        


        private void HLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void HLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show("Hyperlink was clicked");
        }

        private void GetRichTextBoxSpecsButton_Click(object sender, RoutedEventArgs e)
        {
            var doc = CurrentFactRTB.Document;
            var numberOfBlocks = doc.Blocks.Count();
            var content = new TextRange(doc.ContentStart, doc.ContentEnd).Text;

            //below: works :D sets all content to bold
            //doc.FontWeight = FontWeights.Bold;
        }

        /// <summary>
        /// method for testing purposes
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void SaveRTBContent(object Sender, RoutedEventArgs e)
        {
            var range = new TextRange(CurrentFactRTB.Document.ContentStart, CurrentFactRTB.Document.ContentEnd);
            var path = @"C:\\Users\\ATeeu\\source\\repos\\TextTagger_DeDomijnen\\TextTagger_DeDomijnen\\SavedFiles\\TestSave.xaml";
            var fStream = new FileStream(path, FileMode.Create);

            range.Save(fStream, DataFormats.XamlPackage);
            fStream.Close();
        }

        /// <summary>
        /// method for testing purposes
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        //private void LoadRTBContent(object Sender, RoutedEventArgs e)
        //{
        //    TextRange range;
        //    FileStream fStream;
        //    var path = @"C:\\Users\\ATeeu\\source\\repos\\TextTagger_DeDomijnen\\TextTagger_DeDomijnen\\SavedFiles\\TestSave.xaml";

        //    if (File.Exists(path))
        //    {
        //        range = new TextRange(factRichTB.Document.ContentStart, factRichTB.Document.ContentEnd);
        //        fStream = new FileStream(path, FileMode.OpenOrCreate);
        //        range.Load(fStream, DataFormats.XamlPackage);
        //        fStream.Close();
        //    }           
        //}

        //https://www.youtube.com/watch?v=sHD5j8ZUFBs

        private void HeaveContentButton_Click(object sender, RoutedEventArgs e)
        {
            //var loadedDoc = fDocReader.Document;
            //var editableDoc = richTB.Document;

            //editableDoc.Blocks.AddRange(loadedDoc.Blocks); //throws was edited exception

            //for (int i = 0; i < loadedDoc.Blocks.Count; i++)
            //{
            //    editableDoc.Blocks.Add(loadedDoc.Blocks.ElementAt(i));
            //}
        }

        private void FindImagesButton_Click(object sender, RoutedEventArgs e)
        {
            //var loadedDoc = fDocReader.Document;
            //if (loadedDoc != null)
            //{
            //    var images = FindImages(loadedDoc);
            //}           
        }

        /// <summary>
        /// Scans a FlowDocument and returns all Image(s)
        /// inside it as a a new IEnumerable collection.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>collection of images</returns>
        public static IEnumerable<Image> FindImages(FlowDocument document)
        {
            return document.Blocks.SelectMany(block => FindImages(block));
        }

        /// <summary>
        /// Helper method that scans each block in search of Images
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static IEnumerable<Image> FindImages(Block block)
        {
            if (block is Table)
            {
                return ((Table)block).RowGroups
                    .SelectMany(x => x.Rows)
                    .SelectMany(x => x.Cells)
                    .SelectMany(x => x.Blocks)
                    .SelectMany(innerBlock => FindImages(innerBlock));
            }
            if (block is Paragraph)
            {
                return ((Paragraph)block).Inlines
                    .OfType<InlineUIContainer>()
                    .Where(x => x.Child is Image)
                    .Select(x => x.Child as Image);
            }
            if (block is BlockUIContainer)
            {
                return !(((BlockUIContainer)block).Child is Image i)
                            ? new List<Image>()
                            : new List<Image>(new[] { i });
            }
            if (block is List)
            {
                return ((List)block).ListItems.SelectMany(listItem => listItem
                                                                      .Blocks
                                                                      .SelectMany(innerBlock => FindImages(innerBlock)));
            }
            throw new InvalidOperationException("Unknown block type: " + block.GetType());
        }

        #endregion
    }
}
