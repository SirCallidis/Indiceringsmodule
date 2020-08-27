using Indiceringsmodule.Common;
using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Indiceringsmodule.Presentation
{
    public class IndiceringsmoduleTestViewModel : Observable, IDisposable
    {
        public EventAggregator ea;
        private string _TestString;
        private readonly List<Subscription> Subscriptions = new List<Subscription>();
        private FlowDocument _CurrentDocument;
        private FlowDocument _EditableDocument;
        public string TestString
        {
            get { return _TestString; }
            set { SetProperty(ref _TestString, value); }
        }

        public FlowDocument CurrentDocument
        {
            get { return _CurrentDocument; }
            set { SetProperty(ref _CurrentDocument, value); }
        }

        public FlowDocument EditableDocument
        {
            get { return _EditableDocument; }
            set { SetProperty(ref _EditableDocument, value); }
        }
            
        //private string = "<Section xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xml:space="preserve" TextAlignment="Left" LineHeight="Auto" IsHyphenationEnabled="False" xml:lang="en-us" FlowDirection="LeftToRight" NumberSubstitution.CultureSource="Text" NumberSubstitution.Substitution="AsCulture" FontFamily="Segoe UI" FontStyle="Normal" FontWeight="Normal" FontStretch="Normal" FontSize="12" Foreground="#FF000000" Typography.StandardLigatures="True" Typography.ContextualLigatures="True" Typography.DiscretionaryLigatures="False" Typography.HistoricalLigatures="False" Typography.AnnotationAlternates="0" Typography.ContextualAlternates="True" Typography.HistoricalForms="False" Typography.Kerning="True" Typography.CapitalSpacing="False" Typography.CaseSensitiveForms="False" Typography.StylisticSet1="False" Typography.StylisticSet2="False" Typography.StylisticSet3="False" Typography.StylisticSet4="False" Typography.StylisticSet5="False" Typography.StylisticSet6="False" Typography.StylisticSet7="False" Typography.StylisticSet8="False" Typography.StylisticSet9="False" Typography.StylisticSet10="False" Typography.StylisticSet11="False" Typography.StylisticSet12="False" Typography.StylisticSet13="False" Typography.StylisticSet14="False" Typography.StylisticSet15="False" Typography.StylisticSet16="False" Typography.StylisticSet17="False" Typography.StylisticSet18="False" Typography.StylisticSet19="False" Typography.StylisticSet20="False" Typography.Fraction="Normal" Typography.SlashedZero="False" Typography.MathematicalGreek="False" Typography.EastAsianExpertForms="False" Typography.Variants="Normal" Typography.Capitals="Normal" Typography.NumeralStyle="Normal" Typography.NumeralAlignment="Normal" Typography.EastAsianWidths="Normal" Typography.EastAsianLanguage="Normal" Typography.StandardSwashes="0" Typography.ContextualSwashes="0" Typography.StylisticAlternates="0"><Paragraph TextAlignment="Center" FontStyle="Italic" FontWeight="Bold" FontSize="29.333333333333332" Margin="0,0,0,0"><Span FontFamily="Georgia" Foreground="#FF606060"><Run>Alice's Adventures</Run></Span></Paragraph><Paragraph TextAlignment="Center" FontStyle="Italic" FontWeight="Bold" FontSize="29.333333333333332" Margin="0,0,0,0"><Span FontFamily="Georgia" Foreground="#FF606060"><Run>In Wonderland</Run></Span></Paragraph><Paragraph FontSize="16" Margin="0,0,0,0"></Paragraph><Paragraph TextAlignment="Right" FontSize="16" Margin="0,0,0,0"><Span FontFamily="Georgia" FontStyle="Italic" Foreground="#FF909090"><Run>By Lewis Carroll</Run></Span></Paragraph><Paragraph TextAlignment="Right" FontSize="16" Margin="0,0,0,0"></Paragraph><Paragraph TextAlignment="Center" FontStyle="Italic" FontSize="22.666666666666664" Margin="0,0,0,0"><Run> </Run></Paragraph><Paragraph TextAlignment="Center" FontFamily="Times New Roman" FontSize="13.333333333333332" Margin="0,0,0,0"><Span FontStyle="Italic" FontSize="22.666666666666664" Foreground="#FF606060"><Run>Chapter I. Down the Rabbit-Hole</Run></Span></Paragraph><Paragraph TextAlignment="Center" FontSize="16" Margin="0,0,0,0"></Paragraph><Paragraph TextAlignment="Justify" FontFamily="Times New Roman" FontSize="18.666666666666664" Margin="0,0,0,0"><Span FontFamily="Segoe UI" FontSize="16"><Run>      </Run></Span><Span><Run>Alice was beginning to get very tired of sitting by her sister on the bank, and of having nothing to do: once or twice she had peeped into the book her sister was reading, but it had no pictures or conversations in it, `and what is the use of a book,' thought Alice `without pictures or conversation?'</Run></Span></Paragraph><Paragraph TextAlignment="Justify" FontSize="15.333333333333332" Margin="0,0,0,0"><Span FontFamily="Times New Roman" FontSize="18.666666666666664"><Run>      So she was considering in her own mind (as well as she could, for the hot day made her feel very sleepy and stupid), whether the pleasure of making a daisy- chain would be worth the trouble of getting up and picking the daisies, when suddenly a white Rabbit with pink eyes ran close by her.</Run></Span></Paragraph><Paragraph TextAlignment="Center" FontSize="16" Margin="0,0,0,0"><Run> </Run></Paragraph><Paragraph TextAlignment="Justify" FontFamily="Times New Roman" FontSize="18.666666666666664" Margin="0,0,0,0"><Span FontFamily="Segoe UI" FontSize="16"><Run>      </Run></Span><Span><Run>There was nothing so very remarkable in that; nor did Alice think it so very much out of the way to hear the Rabbit say to itself, `Oh dear! Oh dear! I shall be late!' (when she thought it over afterwards, it occurred to her that she ought to have wondered at this, but at the time it all seemed quite natural); but when the Rabbit actually took a watch out of its waistcoat- pocket, and looked at it, and then hurried on, Alice started to her feet, for it flashed across her mind that she had never before seen a rabbit with either a waistcoat-pocket, or a watch to take out of it, and burning with curiosity, she ran across the field after it, and fortunately was just in time to see it pop down a large rabbit-hole under the hedge.</Run></Span></Paragraph><Paragraph TextAlignment="Justify" FontFamily="Times New Roman" FontSize="18.666666666666664" Margin="0,0,0,0"><Span><Run>      In another moment down went Alice after it, never once considering how in the world she was to get out again. The rabbit-hole went straight on like a tunnel for some way, and then dipped suddenly down, so suddenly that Alice had not a moment to think about stopping herself before she found herself falling down a very deep well.</Run></Span></Paragraph><Paragraph TextAlignment="Justify" FontFamily="Times New Roman" FontSize="18.666666666666664" Margin="0,0,0,0"><Span><Run>      Either the well was very deep, or she fell very slowly, for she had plenty of time as she went down to look about her and to wonder what was going to happen next. First, she tried to look down and make out what she was coming to, but it was too dark to see anything; then she looked at the sides of the well, and noticed that they were filled with cupboards and book-shelves; here and there she saw maps and pictures hung upon pegs. She took down a jar from one of the shelves as she passed; it was labelled `ORANGE MARMALADE', but to her great disappointment it was empty: she did not like to drop the jar for fear of killing somebody, so managed to put it into one of the cupboards as she fell past it.</Run></Span></Paragraph><Paragraph TextAlignment="Justify" FontFamily="Times New Roman" FontSize="18.666666666666664" Margin="0,0,0,0"><Span><Run>      `Well!' thought Alice to herself, `after such a fall as this, I shall think nothing of tumbling down stairs! How brave they'll all think me at home! Why, I wouldn't say anything about it, even if I fell off the top of the house!' (Which was very likely true.)</Run></Span></Paragraph><Paragraph TextAlignment="Justify" FontFamily="Times New Roman" FontSize="18.666666666666664" Margin="0,0,0,0"></Paragraph></Section>";


        public RelayCommand Button_Click { get; private set; }
        public RelayCommand Button_ClearLeft { get; private set; }

        public IndiceringsmoduleTestViewModel(EventAggregator ea)
        {
            this.ea = ea;           
            WireUpForm();
        }

        private void WireUpForm()
        {
            Subscriptions.Add(ea.Subscribe<ButtonClickEventModel>(m => MessageReceived(m.Data)));
            Subscriptions.Add(ea.Subscribe<DocumentLoadedEventModel>(m => DisplayNewCurrentDocument(m.Document)));
            Subscriptions.Add(ea.Subscribe<RequestDocumentForSavingEventModel>(m => MakeDocumentAvailable()));
            Button_Click = new RelayCommand(OnClick, CanClick);
            Button_ClearLeft = new RelayCommand(OnClearLeft, CanClearLeft);
            EditableDocument = new FlowDocument();

            Paragraph par = new Paragraph();
            Run run1 = new Run("Programmatically added text.");
            par.Inlines.Add(run1);

            EditableDocument.Blocks.Add(par);
        }

        /// <summary>
        /// Checks wether the CurrentDocument property is null,
        /// if it is, it sets the document parameter as the new CurrentDocument.
        /// If it isn't, it'll append the document parameter to the existing
        /// CurrentDocument.
        /// </summary>
        /// <param name="document"></param>
        private void DisplayNewCurrentDocument(FlowDocument document)
        {
            if (CurrentDocument == null)
            {
                CurrentDocument = document;
            }
            else
            {
                for (int i = 0; i < document.Blocks.Count; i++)
                {
                    CurrentDocument.Blocks.Add(document.Blocks.ElementAt(i));
                }
            }           
        }

        private bool CanClick()
        {
            return true;
        }

        private void OnClick()
        {
            if (CanClick())
            {
                //just an example of ea.Publishing
                ea.Publish(new ButtonClickEventModel() { Data = "Boop!" });
            }
        }

        private bool CanClearLeft()
        {
            return true;
        }

        private void OnClearLeft()
        {
            if (CanClearLeft())
            {
                CurrentDocument = null;
            }
        }

        private void MessageReceived(string data)
        {
            TestString = data;
        }

        private void MakeDocumentAvailable()
        {
            ea.Publish(new PublishDocumentEventModel() { Document = EditableDocument });
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
