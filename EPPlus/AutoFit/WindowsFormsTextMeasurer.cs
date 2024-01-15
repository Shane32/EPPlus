/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 *
 * Copyright (C) 2024 Shane Krueger
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 *
 * The GNU Lesser General Public License can be viewed at http://www.opensource.org/licenses/lgpl-license.php
 * If you unfamiliar with this license or have questions about it, here is an http://www.gnu.org/licenses/gpl-faq.html
 *
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 *
 * Code change notes:
 * 
 * Author                           Change                      Date
 *******************************************************************************
 * Shane Krueger                    Added                       12-JAN-2024
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;

namespace OfficeOpenXml.AutoFit
{
#if NET6_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    /// <summary>
    /// Measures text using the Windows Forms Graphics.MeasureString method.
    /// </summary>
    public class WindowsFormsTextMeasurer : ITextMeasurer
    {
        private readonly Dictionary<FontInfo, Font> _fonts = new Dictionary<FontInfo, Font>();
        private readonly Bitmap _bitmap = new Bitmap(1, 1);
        private readonly Graphics _graphics;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsFormsTextMeasurer"/> class.
        /// </summary>
        public WindowsFormsTextMeasurer()
        {
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.PageUnit = GraphicsUnit.Pixel;
        }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        public SizeF MeasureString(string text, string fontName, float size, bool bold, bool italic, bool underline, bool strikeout, int maxWidth = -1)
        {
            if (text == null)
                text = "";
            if (fontName == null)
                throw new ArgumentNullException(nameof(fontName));
            return MeasureString(text, new FontInfo(fontName, size, bold, italic, underline, strikeout), maxWidth);
        }

        private SizeF MeasureString(string text, FontInfo fontInfo, int maxWidth = -1)
        {
            if (maxWidth == -1)
                maxWidth = 99999;
            if (!_fonts.TryGetValue(fontInfo, out var font))
            {
                font = new Font(fontInfo.Name, fontInfo.Size, fontInfo.Style);
                _fonts.Add(fontInfo, font);
            }
            if (text.Length > 32000) text = text.Substring(0, 32000); //Issue
            return _graphics.MeasureString(text, font, maxWidth, StringFormat.GenericDefault);
        }

        private struct FontInfo
        {
            public string Name;
            public float Size;
            public FontStyle Style;

            public FontInfo(string name, float size, FontStyle style)
            {
                Name = name;
                Size = size;
                Style = style;
            }

            public FontInfo(string name, float size, bool bold, bool italic, bool underline, bool strikeout)
                : this(name, size, FontStyle.Regular 
                      | (bold ? FontStyle.Bold : 0)
                      | (italic ? FontStyle.Italic : 0)
                      | (underline ? FontStyle.Underline : 0)
                      | (strikeout ? FontStyle.Strikeout : 0))
            {
            }
        }

        /// <inheritdoc/>
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
