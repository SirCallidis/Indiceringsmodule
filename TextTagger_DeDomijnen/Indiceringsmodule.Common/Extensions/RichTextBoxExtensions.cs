using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Indiceringsmodule.Common.Extensions
{
    public static class RichTextBoxExtensions
    {
        public static void PrevKeyDown(this RichTextBox rtb, KeyEventArgs e)
        {
            var caretPos = rtb.CaretPosition;
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                caretPos.InsertLineBreak();
            }
        }

        /// <summary>
        /// Takes in a RichTextBox as a parameter 
        /// crops its Selection until no leading or
        /// trailing whitespaces remain.
        /// </summary>
        /// <param name="rtb"></param>
        public static void CropSelectionWhitespace(this RichTextBox rtb)
        {
            if (rtb.Selection == null) throw new ArgumentException("RichTextBox does not have a Selection to process!");

            var selection = rtb.Selection;

            var hasTrailingWhitespace = char.IsWhiteSpace(selection.Text[selection.Text.Length - 1]);
            while (hasTrailingWhitespace)
            {
                rtb.Selection.Select(selection.Start, selection.End.GetPositionAtOffset(-1));
                hasTrailingWhitespace = char.IsWhiteSpace(selection.Text[selection.Text.Length - 1]);
            }

            var hasLeadingWhitespace = char.IsWhiteSpace(selection.Text[0]);
            while (hasLeadingWhitespace)
            {
                rtb.Selection.Select(selection.Start.GetPositionAtOffset(1), selection.End);
                hasLeadingWhitespace = char.IsWhiteSpace(selection.Text[0]);
            }
        }
    }
}
