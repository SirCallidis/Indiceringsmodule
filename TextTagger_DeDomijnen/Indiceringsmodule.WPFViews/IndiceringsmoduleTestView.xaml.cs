using Indiceringsmodule.Common;
using Indiceringsmodule.DataAccess;
using Indiceringsmodule.Presentation;
using System;
using System.Collections.Generic;
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
    public partial class IndiceringsmoduleTestView : UserControl
    {
        // TODO - clean up this code-behind
        private protected EventAggregator ea;


        public IndiceringsmoduleTestView()
        {
            InitializeComponent();
            

            //TODO - below: breaks because constructor hasn't finished yet => after finish it sets the datacontext in markup
            //solution: on-startup method, like in charcreator
            //var dCon = DataContext as IndiceringsmoduleTestViewModel;
            //this.ea = dCon.ea;
        }

        public void SetEaOnViewLoaded()
        {
            var dCon = DataContext as IndiceringsmoduleTestViewModel;
            this.ea = dCon.ea;
        }


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
            var loadedDoc = fDocReader.Document;
            var editableDoc = richTB.Document;

            //editableDoc.Blocks.AddRange(loadedDoc.Blocks); //throws was edited exception

            for (int i = 0; i < loadedDoc.Blocks.Count; i++)
            {
                editableDoc.Blocks.Add(loadedDoc.Blocks.ElementAt(i));
            }
        }

        private void FindImagesButton_Click(object sender, RoutedEventArgs e)
        {
            var loadedDoc = fDocReader.Document;
            if (loadedDoc != null)
            {
                var images = FindImages(loadedDoc);
            }           
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

    }
}
