using Indiceringsmodule.Common.EventModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Indiceringsmodule.Common
{
    public class RichFactTextBox : RichTextBox
    {
        private readonly EventAggregator Ea;

        #region default constuctors

        public RichFactTextBox()
        {

        }

        public RichFactTextBox(EventAggregator ea)
        {
            Ea = ea;
        }

        #endregion

        /// <summary>
        /// Checks which key was pressed and handles
        /// accordingly.
        /// </summary>
        /// <param name="rtb"></param>
        /// <param name="e"></param>
        public void PrevKeyDown(RichFactTextBox rtb, KeyEventArgs e)
        {
            var caretPos = rtb.CaretPosition;
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                caretPos.InsertLineBreak();
            }
            if (e.Key == Key.Back)
            {
                ResolveBackspaceKey(rtb, e);
            }
        }

        private void ResolveBackspaceKey(RichFactTextBox rtb, KeyEventArgs e)
        {
            var caretPos = rtb.CaretPosition;
            var textBeforeCaret = caretPos.GetTextInRun(LogicalDirection.Backward);
            if (caretPos.Parent is Run run)
            {
                if (textBeforeCaret == "")
                {
                    var previousRun = run.PreviousInline ?? new Run();
                    if (previousRun.GetType() == typeof(Hyperlink))
                    {
                        var hLink = previousRun as Hyperlink;
                        var hLinkRun = hLink.Inlines.FirstInline as Run;
                        if (hLinkRun.Text.Length == 1)
                        {
                            var lastHyperlinkCharacterData = new LastHyperlinkCharacterData
                            {
                                rtb = rtb,
                                hyperlink = hLink,
                            };
                            Ea.Publish(new WarningLastHyperlinkCharacterEventModel() { Data = lastHyperlinkCharacterData });
                            //TODO make someone listen to this
                            e.Handled = true;
                        }
                    }
                }
                else //if the caret is not at the start of the Run
                {
                    var list = caretPos.Paragraph.Inlines.ToList();
                    var indexOfRun = list.IndexOf(run);
                    if (indexOfRun != -1)
                    {
                        var indexOfPrevRun = 0;
                        if (indexOfRun != 0 || indexOfRun > 0)
                        {
                            indexOfPrevRun = indexOfRun - 1;
                        }
                        var prevRun = list[indexOfPrevRun];
                        if (textBeforeCaret == "")
                        {
                            if (prevRun.GetType() == typeof(Hyperlink))
                            {
                                if (run.Text.Length == 1)
                                {
                                    //MessageBox.Show("last character!");
                                    //event
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a Hyperlink object out of the text selection and signals the
        /// creation of a new FactMember.
        /// </summary>
        /// <param name="rtb"></param>
        public void CreateHyperlink(RichTextBox rtb, bool isMultiLine, string factMemberType, FlowDocument currentFact)
        {
            var selection = rtb.Selection;
            var firstStringPart = selection.Start.GetTextInRun(LogicalDirection.Backward);
            var lastStringPart = selection.End.GetTextInRun(LogicalDirection.Forward);

            //create two runs out of the latter part of the original run
            var r1 = new Run(selection.Text);
            var r2 = new Run(lastStringPart);

            //create a hyperlink
            Hyperlink factMemberLink = new Hyperlink(r1);
            factMemberLink.Click += HLink_Click;

            //signal creation of new factmember
            var factMemberCreationData = new FactMemberCreationData
            {
                CurrentFact = currentFact,
                Hyperlink = factMemberLink,
                ChosenType = factMemberType,
                Selection = selection.Text,
            };
            Ea.Publish(new CreateFactMemberEventModel() { Data = factMemberCreationData });

            //find the end of the current line
            var nextStart = selection.Start.GetLineStartPosition(1);
            var lineEnd = (nextStart ?? selection.Start.DocumentEnd).GetInsertionPosition(LogicalDirection.Backward);

            //find the amount of character between start of selection and end of line
            var charCount = selection.Start.GetOffsetToPosition(nextStart);

            //delete the rest of the Run content, basically leaving firstStringPart
            selection.Start.DeleteTextInRun(charCount);

            //find position of edited run in context to the other inlines in its paragraph
            var containingPar = selection.Start.Paragraph;
            var inlines = containingPar.Inlines.ToList();
            var currentRun = selection.Start.GetPositionAtOffset(0).Parent as Run;
            var runIndex = inlines.IndexOf(currentRun);

            //insert the Hyperlink and Run Inline after the proper index.
            inlines.Insert(runIndex + 1, factMemberLink);
            inlines.Insert(runIndex + 2, r2);

            if (isMultiLine)
            {
                inlines.RemoveRange(runIndex + 3, 2);
            }

            //Repopulate the InlinesCollection with items from the List.
            containingPar.Inlines.Clear();
            foreach (var newInline in inlines)
            {
                containingPar.Inlines.Add(newInline);
            }
        }

        private void HLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Ea.Publish(new HyperlinkClickedEventModel() { Data = sender as Hyperlink });
        }
    }
}
