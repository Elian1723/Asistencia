using System.Drawing;
using Excel = Microsoft.Office.Interop.Excel;

namespace Asistencia
{
    public static class RangeExtensions
    {
        public static Excel.Range SetFontName(this Excel.Range range, string fontName)
        {
            range.Font.Name = fontName;
            return range;
        }

        public static Excel.Range SetFontSize(this Excel.Range range, int fontSize)
        {
            range.Font.Size = fontSize;
            return range;
        }

        public static Excel.Range SetFontBold(this Excel.Range range, bool isBold)
        {
            range.Font.Bold = isBold;
            return range;
        }
        public static Excel.Range SetHorizontalAlignment(this Excel.Range range, Excel.XlHAlign align)
        {
            range.HorizontalAlignment = align;
            return range;
        }

        public static Excel.Range SetBackgroundColor(this Excel.Range range, string colorHex)
        {
            var color = ColorTranslator.FromHtml(colorHex);
            int oleColor = color.R | (color.G << 8) | (color.B << 16);

            range.Interior.Color = oleColor;
            return range;
        }

        public static Excel.Range SetBorder(this Excel.Range range, string colorHex)
        {
            var color = ColorTranslator.FromHtml(colorHex);
            int oleColor = color.R | (color.G << 8) | (color.B << 16);

            range.Borders.Color = oleColor;
            return range;
        }
        public static Excel.Range SetLeftBorder(this Excel.Range range, string colorHex)
        {
            var color = ColorTranslator.FromHtml(colorHex);
            int oleColor = color.R | (color.G << 8) | (color.B << 16);
            range.Borders[Excel.XlBordersIndex.xlEdgeLeft].Color = oleColor;
            return range;
        }

        public static Excel.Range SetTopBorder(this Excel.Range range, string colorHex)
        {
            var color = ColorTranslator.FromHtml(colorHex);
            int oleColor = color.R | (color.G << 8) | (color.B << 16);
            range.Borders[Excel.XlBordersIndex.xlEdgeTop].Color = oleColor;
            return range;
        }

        public static Excel.Range SetRightBorder(this Excel.Range range, string colorHex)
        {
            var color = ColorTranslator.FromHtml(colorHex);
            int oleColor = color.R | (color.G << 8) | (color.B << 16);
            range.Borders[Excel.XlBordersIndex.xlEdgeRight].Color = oleColor;
            return range;
        }

        public static Excel.Range SetBottomBorder(this Excel.Range range, string colorHex)
        {
            var color = ColorTranslator.FromHtml(colorHex);
            int oleColor = color.R | (color.G << 8) | (color.B << 16);
            range.Borders[Excel.XlBordersIndex.xlEdgeBottom].Color = oleColor;
            return range;
        }


    }
}
