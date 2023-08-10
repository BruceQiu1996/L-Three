using System.Linq;
using System.Windows;
using System.Windows.Media;
using ThreeL.Client.Shared.Entities;

namespace ThreeL.Client.Win.ViewModels.Messages
{
    public class TextMessageViewModel : MessageViewModel
    {
        public string Text { get; set; }

        public override string GetShortDesc()
        {
            if (MeasureTextWidth(Text, 13, "微软雅黑") <= 105)
                return Text;

            string temp = Text.Substring(0, 6);
            foreach (var index in Enumerable.Range(7, Text.Length - 7))
            {
                string str = Text.Substring(0, index);
                var len = MeasureTextWidth(str, 13, "微软雅黑");
                if (len > 105)
                    break;

                temp = str;
            }

            return $"{temp}..";
        }

        private double MeasureTextWidth(string text, double fontSize, string fontFamily)
        {
            FormattedText formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily.ToString()),
            fontSize,
            Brushes.Black
            );
            return formattedText.WidthIncludingTrailingWhitespace;
        }

        public override void FromEntity(ChatRecord chatRecord)
        {
            base.FromEntity(chatRecord);
            Text = chatRecord.Message;
        }
    }
}
