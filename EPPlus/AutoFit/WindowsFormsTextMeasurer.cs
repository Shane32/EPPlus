using OfficeOpenXml.Style.XmlAccess;
using System.Collections.Generic;
using System.Drawing;

namespace OfficeOpenXml.AutoFit
{
#if NET6_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public class WindowsFormsTextMeasurer : ITextMeasurer
    {
        private readonly Bitmap _bitmap;
        private readonly Graphics _graphics;
        private readonly Dictionary<ExcelFontXml, Font> _fonts;

        public WindowsFormsTextMeasurer()
        {
            _bitmap = new Bitmap(1, 1);
            _graphics = Graphics.FromImage(_bitmap);
        }

#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        public SizeF MeasureString(string text, ExcelFontXml excelFont, int width)
        {
            if (!_fonts.TryGetValue(excelFont, out var font))
            {
                var fs = FontStyle.Regular;
                if (excelFont.Bold) fs |= FontStyle.Bold;
                if (excelFont.UnderLine) fs |= FontStyle.Underline;
                if (excelFont.Italic) fs |= FontStyle.Italic;
                if (excelFont.Strike) fs |= FontStyle.Strikeout;
                font = new Font(excelFont.Name, excelFont.Size, fs);
                _fonts.Add(excelFont, font);
            }
            return _graphics.MeasureString(text, font, width, StringFormat.GenericDefault);
        }

        public void Dispose()
        {
            foreach (var font in _fonts.Values)
            {
                font.Dispose();
            }
            _graphics.Dispose();
            _bitmap.Dispose();
        }
    }
}
