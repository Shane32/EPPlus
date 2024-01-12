using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Drawing;

namespace OfficeOpenXml.AutoFit
{
    /// <summary>
    /// An interface to measure text of a specified font, size, and style.
    /// </summary>
    public interface ITextMeasurer : IDisposable
    {
        /// <inheritdoc cref="System.Drawing.Graphics.MeasureString(string?, Font, int)"/>
        SizeF MeasureString(string text, ExcelFontXml excelFont, int width);
    }
}
