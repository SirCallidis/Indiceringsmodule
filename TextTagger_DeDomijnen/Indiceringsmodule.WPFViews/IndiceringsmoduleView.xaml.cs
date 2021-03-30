using Indiceringsmodule.Common;
using Indiceringsmodule.Common.DocumentObject;
using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.Common.Extensions;
using Indiceringsmodule.Presentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Indiceringsmodule.WPFViews
{
    /// <summary>
    /// TODO: Contains a lot of logic. Much of it could be refactored to go into the
    /// viewmodel and assorted classes instead.
    /// </summary>
    public partial class IndiceringsmoduleView : UserControl
    {
        #region Fields & Properties
            
        private protected EventAggregator Ea;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();
        private double oldSize;
        private Dictionary<int, RichTextBox> FactDocumentUIs;
        public RichTextBox CurrentFactRTB;

        #endregion Fields & Properties

        #region Default Constructor

        public IndiceringsmoduleView(EventAggregator ea)
        {
            Ea = ea;
            InitializeComponent();
            WireUpForm();
        }

        /// <summary>
        /// Wires up the assorted parts of the class to a working whole.
        /// </summary>
        private void WireUpForm()
        {
            SetRichTBTextSettings(transcriptionRichTB.Document);
            CurrentFactRTB = CreateNewFactRTB();
            SetRichTBTextSettings(CurrentFactRTB.Document);
            ResetSlider();

            Subscriptions.Add(Ea.Subscribe<UpdateViewEventModel>(m => UpdateView(m.Data)));
            Subscriptions.Add(Ea.Subscribe<DocumentLoadedEventModel>(m => LoadedFlowDocumentReceived(m.Data)));
            Subscriptions.Add(Ea.Subscribe<PublishFactEventModel>(m => UpdateUIOnFactReceived(m.Data)));
            Subscriptions.Add(Ea.Subscribe<NewFactWasCreatedEventModel>(m => OnNewFactCreated(m.Data)));
            Subscriptions.Add(Ea.Subscribe<ProvidingViewForFactMemberEventModel>(m => ResolveView(m.Data)));
            Subscriptions.Add(Ea.Subscribe<RetrieveDocsFromViewEventModel>(m => AddFlowDocumentsToDocumentObject(m.Data)));
            FactDocumentUIs = new Dictionary<int, RichTextBox>();
        }

        #endregion Default Constructor

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
                Data = RetrieveFactDocumentFromView(),
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
                Data = RetrieveFactDocumentFromView(),
                Direction = Enums.direction.previous,
            });
        }

        /// <summary>
        /// Returns the FlowDocument currently displayed on the View.
        /// Should this be null, will return a new empty FlowDocument.
        /// </summary>
        /// <returns></returns>
        private FlowDocument RetrieveFactDocumentFromView()
        {
            FlowDocument data;
            if (RTBDisplayer.Content == null)
            {
                data = new FlowDocument();
            }
            else
            {
                var rtbContent = RTBDisplayer.Content as RichFactTextBox;
                data = rtbContent.Document;
            }
            return data;
        }

        /// <summary>
        /// Determines whether to handle the PrevKeyDown method on either
        /// the RichTextBox, or RichFactTextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender.GetType() == typeof(RichTextBox))
            {
                var rtb = sender as RichTextBox;
                rtb.PrevKeyDown(e);
            }
            if (sender.GetType() == typeof(RichFactTextBox))
            {
                var rftb = sender as RichFactTextBox;
                rftb.PrevKeyDown(rftb, e);
            }
        }

        /// <summary>
        /// Displays the incoming UserControl in the FactMemberDisplayer on the View.
        /// </summary>
        /// <param name="data"></param>
        private void ResolveView(UserControl data)
        {
            FactMemberDisplayer.Content = data;
        }

        /// <summary>
        /// Sets the view's edited documents: transcription document and
        /// each of the fact's documents, to the correct properties of the
        /// ViewModel's DocumentObject.
        /// </summary>
        /// <param name="docOb"></param>
        private void AddFlowDocumentsToDocumentObject(DocumentObject docOb)
        {
            Dispatcher.Invoke(() =>
            {
                docOb.TranscriptionDocument = transcriptionRichTB.Document;
                for (int i = 0; i < FactDocumentUIs.Count; i++)
                {
                    var rtb = FactDocumentUIs[i];
                    docOb.TotalFacts[i].FactDocument = rtb.Document;
                }
                Ea.Publish(new PublishDocumentEventModel() { Data = docOb });
            });
        }

        #endregion General Helper Methods

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
            var context = DataContext as IndiceringsmoduleViewModel;
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

        #endregion Methods dealing with Images

        #region Methods dealing with RichTextbox(es)

        /// <summary>
        /// Makes sure only the pasted text as string is
        /// filtered to be pasted into richtextbox.
        /// This omits all textformatting data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnPaste(object sender, DataObjectPastingEventArgs e)
        //{
        //    if (transcriptionRichTB.IsFocused == true)
        //    {
        //        var caret = transcriptionRichTB.CaretPosition;
        //        var selection = e.DataObject.GetData(DataFormats.Xaml);
        //        string pastingText = e.DataObject.GetData(DataFormats.Text) as string;
        //        var lines = Regex.Split(pastingText, "\r\n").ToArray();
        //        //lines.Reverse(); //<== if contains linebreak, no reverse, else, reverse!
        //        for (int i = lines.Length; i > 0; i--)
        //        {
        //            caret.InsertTextInRun(lines[i - 1] + " ");
        //        }
        //        //if (pastingText.Substring(0, 4) == "\r\n")
        //        //{
        //        //    transcriptionRichTB.CaretPosition.InsertLineBreak();
        //        //}
        //        //foreach (var line in lines)
        //        //{
        //        //    caret.InsertTextInRun(line + " ");
        //        //}
        //        e.CancelCommand();
        //    }
        //    if (CurrentFactRTB.IsFocused == true)
        //    {
        //        string pastingText = e.DataObject.GetData(DataFormats.Text) as string;
        //        var lines = Regex.Split(pastingText, "\r\n").ToList();
        //        var p = CurrentFactRTB.Document.ContentStart.Paragraph ?? new Paragraph();
        //        foreach (var line in lines)
        //        {
        //            var run = new Run(line);
        //            p.Inlines.Add(run);
        //            p.Inlines.Add(new LineBreak());
        //        }
        //        CurrentFactRTB.Document.Blocks.Add(p);
        //        e.CancelCommand();
        //    }
        //    else
        //    {
        //        transcriptionRichTB.Focus();
        //    }

        //}

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
            SetRichTBTextSettings(CurrentFactRTB.Document);
        }

        /// <summary>
        /// Creates a new RichFactTextBox with
        /// hardcoded settings
        /// </summary>
        /// <returns></returns>
        private RichFactTextBox CreateNewFactRTB()
        {
            var rtb = new RichFactTextBox(Ea)
            {
                IsEnabled = true,
                Padding = new Thickness(5),
            };
            rtb.IsDocumentEnabled = true;
            rtb.PreviewKeyDown += RTB_PreviewKeyDown;
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

        #endregion Methods dealing with RichTextbox(es)

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
            //could be refactored to go into RichTextBoxExtensions,
            //needs a global MessageBoxEvent first
            var rtb = (RichTextBox)RTBDisplayer.Content;
            if (rtb != null)
            {
                var selection = rtb.Selection.Text;
                if (selection == "")
                {
                    MessageBox.Show("*nothing was selected.");
                    return false;
                }

                if (selection.Length < 1)
                {
                    MessageBox.Show("*Selection is too short.");
                    return false;
                }
                if (selection.Length > 50)
                {
                    MessageBox.Show("*Selection is too long.");
                    return false;
                }

                if (FactMemberSelector.SelectedItem == null)
                {
                    MessageBox.Show("*Must select the type of Fact Member first!");
                    return false;
                }

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
            return false;
        }

        /// <summary>
        /// Takes care of leading and trailing whitespaces, then calls
        /// Hyperlink creation based on whether Selection has a line ending in it.
        /// </summary>
        private void SignalCreateFactMember()
        {
            var currentFactRTB = (RichFactTextBox)RTBDisplayer.Content;

            currentFactRTB.CropSelectionWhitespace();

            var isMultiLine = currentFactRTB.Selection.Text.Contains("\r\n");
            currentFactRTB.CreateHyperlink( currentFactRTB, 
                                            isMultiLine, 
                                            FactMemberSelector.SelectedItem.ToString(), 
                                            currentFactRTB.Document);
        }

        #endregion Methods dealing with Fact and FactMember creation

        #region Temporary and Testing Methods

        /*
         ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
         Below: unused and testing methods. Some code may be handy later.
         Make sure nothing calls these methods outside of temporary tests.
         ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        */

        /// <summary>
        /// general method for quick testing, its purpose changes
        /// with each iteration. Not intended for use on Release.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            //var selection = CurrentFactRTB.Selection.Text;

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
            //hLink.Click += HLink_Click;
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
        /// High level method for finding images in a FlowDocument.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #endregion Temporary and Testing Methods
    }
}
