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

        #endregion

        #region Default Constructor

        public IndiceringsmoduleView(EventAggregator ea)
        {
            Ea = ea;
            InitializeComponent();
            DataObject.AddPastingHandler(this, OnPaste);
            SetRichTBTextSettings();
            ResetSlider();           

            Subscriptions.Add(Ea.Subscribe<UpdateViewEventModel>(m => UpdateView(m.Data)));
        }

        /// <summary>
        /// Sets the slider halfway, so a positive and negative zoom can be achieved.
        /// </summary>
        private void ResetSlider()
        {
            slider.Value = 0.55;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Takes the text as strings from each Block in the
        /// richTB.Document, Concatonates these to a single string
        /// while saving line endings.
        /// Then wipes the Blocks and creates a new, single
        /// Block and Paragraph with the new large string in it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PutAllTextInOneBlockButton_Click(object sender, RoutedEventArgs e)
        {
            var doc = richTB.Document;
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

            SetRichTBTextSettings();
            
        }

        /// <summary>
        /// Checks if the richTB control has a non-null FlowDocument,
        /// then sets the Font Family, Size, and Weight to hardcoded
        /// values.
        /// </summary>
        private void SetRichTBTextSettings()
        {
            var doc = richTB.Document;
            if (doc == null) throw new ArgumentNullException("Document cannot be null!");
            
            //bot sure yet which is better:
            //foreach (var block in doc.Blocks)
            //{               
            //    block.FontWeight = FontWeights.Normal;
            //    block.FontSize = 14;
            //    block.FontFamily = new FontFamily("Segoe");
            //}
            doc.FontWeight = FontWeights.Normal;
            doc.FontSize = 14;
            doc.FontFamily = new FontFamily("Segoe");
        }

        /// <summary>
        /// Makes sure only the pasted text as string is
        /// filtered to be pasted into richtextbox.
        /// This omits all textformatting data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            string pastingText = e.DataObject.GetData(DataFormats.Text) as string;
            richTB.Document.ContentEnd.InsertTextInRun(pastingText);
            e.CancelCommand();
        }

        /// <summary>
        /// Logic that enables or disables the CreateObject button
        /// based on wether text is selected in the richtextbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTB_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selection = richTB.Selection;
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




        #region Temporary and Testing Methods

        /// <summary>
        /// method for testing purposes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTB.Selection.Text;

            var doc = richTB.Document;
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


            //below: test what happened inside RichTB
            var x = richTB.Document;
            Clipboard.GetText();
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

        /// <summary>
        /// method for testing purposes
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void SaveRTBContent(object Sender, RoutedEventArgs e)
        {
            var range = new TextRange(richTB.Document.ContentStart, richTB.Document.ContentEnd);
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
        private void LoadRTBContent(object Sender, RoutedEventArgs e)
        {
            TextRange range;
            FileStream fStream;
            var path = @"C:\\Users\\ATeeu\\source\\repos\\TextTagger_DeDomijnen\\TextTagger_DeDomijnen\\SavedFiles\\TestSave.xaml";

            if (File.Exists(path))
            {
                range = new TextRange(richTB.Document.ContentStart, richTB.Document.ContentEnd);
                fStream = new FileStream(path, FileMode.OpenOrCreate);
                range.Load(fStream, DataFormats.XamlPackage);
                fStream.Close();
            }           
        }

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

        public static IEnumerable<Image> FindImages(FlowDocument document)
        {
            return document.Blocks.SelectMany(block => FindImages(block));
        }

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

        private void GetRichTextBoxSpecsButton_Click(object sender, RoutedEventArgs e)
        {
            var doc = richTB.Document;
            var numberOfBlocks = doc.Blocks.Count();
            var content = new TextRange(doc.ContentStart, doc.ContentEnd).Text;

            //below: works :D sets all content to bold
            //doc.FontWeight = FontWeights.Bold;
        }

        #endregion


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

        private void ImageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = this.DataContext as IndiceringsmoduleViewModel;
            image.Source = context.SelectedImage;
        }
    }
}
