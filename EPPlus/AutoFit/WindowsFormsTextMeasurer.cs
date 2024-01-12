using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace OfficeOpenXml.AutoFit
{
#if NET6_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public class WindowsFormsTextMeasurer : ITextMeasurer
    {
        private readonly Dictionary<ExcelFontXml, Font> _fonts = new Dictionary<ExcelFontXml, Font>();
        private readonly Bitmap _bitmap = new Bitmap(1, 1);
        private readonly Graphics _graphics;

        public WindowsFormsTextMeasurer()
        {
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.PageUnit = GraphicsUnit.Pixel;
        }

#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        public SizeF MeasureString(string text, ExcelFontXml excelFont, int maxWidth)
        {
            if (text == null)
                text = "";
            if (excelFont == null)
                throw new ArgumentNullException(nameof(excelFont));
            if (maxWidth == -1)
                maxWidth = 99999;
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
            if (text.Length > 32000) text = text.Substring(0, 32000); //Issue
            return _graphics.MeasureString(text, font, maxWidth, StringFormat.GenericDefault);
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
