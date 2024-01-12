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
        /// <summary>
        /// Measures the specified text when drawn with the specified <see cref="Font"/>.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="excelFont"><see cref="ExcelFontXml"/> that defines the text format.</param>
        /// <param name="maxWidth">Maximum width of the string in pixels, or -1 for no maximum.</param>
        /// <returns>
        /// This method returns a <see cref="SizeF"/> structure that represents the size in pixels
        /// of the text specified by the <paramref name="text"/> parameter as drawn
        /// with the <paramref name="excelFont"/> parameter.
        /// </returns>
        SizeF MeasureString(string text, ExcelFontXml excelFont, int maxWidth = -1);
    }
}
