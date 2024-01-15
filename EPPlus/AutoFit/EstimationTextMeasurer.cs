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
 * Shane Krueger                    Added                       15-JAN-2024
 *******************************************************************************/
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace OfficeOpenXml.AutoFit
{
    public class EstimationTextMeasurer : ITextMeasurer
    {
        private static FontSizeEstimationData _staticFontData;
        private static readonly object _staticFontDataLock = new object();
        private readonly FontSizeEstimationData _fontData;

        public EstimationTextMeasurer()
        {
            if (_staticFontData == null)
            {
                lock (_staticFontDataLock)
                {
                    if (_staticFontData == null)
                    {
                        // pull TextMeasurements.dat from the embedded resources
                        // and deserialize it into a FontSizeEstimationData object
                        // for use by all instances of this class
                        using (var stream = typeof(EstimationTextMeasurer).Assembly.GetManifestResourceStream("OfficeOpenXml.AutoFit.TextMeasurements.dat"))
                        {
                            _staticFontData = BinarySerializer.Deserialize(stream);
                        }
                    }
                }
            }
            _fontData = _staticFontData;
        }

        public EstimationTextMeasurer(FontSizeEstimationData fontData)
        {
            _fontData = fontData;
        }

        private readonly static char[] _newLineChars = new char[] { '\n', '\r' };

        /// <inheritdoc/>
        public SizeF MeasureString(string text, string fontName, float size, bool bold, bool italic, bool underline, bool strikethrough, int maxWidth = -1)
        {
            // note: this method does not support maxWidth
            var height = size * 1.5f;
            if (text == null)
            {
                return new SizeF(0, height);
            }
            var font = Find(fontName, bold, italic);
            if (font.FontMetricData == null)
            {
                return new SizeF(0, height);
            }
            height = font.FontMetricData.Height * size / 12f;
            if (text.IndexOfAny(_newLineChars) >= 0)
            {
                var lines = text.Split('\n', '\r');
                var w = lines.Select(x => MeasureString(x, font)).Max();
                return new SizeF(w * size / 12f, height * lines.Length);
            }
            return new SizeF(MeasureString(text, font) * size / 12f, height);
        }

        /// <summary>
        /// Measures the width of the specified text based on the supplied <see cref="ExcelFontMatch"/> metrics.
        /// </summary>
        private float MeasureString(string text, ExcelFontMatch font)
        {
            float w = font.FontMetricData.Padding;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c < ' ')
                {
                    // ignore control characters such as tab, cr, lf
                }
                if (c == '-')
                {
                    w += font.FontMetricData.Dash;
                }
                // Check if the current character is a high surrogate
                else if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    // Combine the high and low surrogates to get the code point
                    int codepoint = char.ConvertToUtf32(text, i);
                    var category = CharUnicodeInfo.GetUnicodeCategory(text, i);

                    // Process the codepoint
                    w += MeasureCodepoint(codepoint, category);

                    // Skip the next character as we have processed a surrogate pair
                    i++;
                }
                else
                {
                    // Process the single character
                    w += MeasureCodepoint(c, CharUnicodeInfo.GetUnicodeCategory(c));
                }
            }
            return w * font.Multiplier;

            float MeasureCodepoint(int codepoint, UnicodeCategory cCategory)
            {
                float l = font.FontMetricData.Unmatched;
                foreach (var category in font.FontMetricData.Categories)
                {
                    if (category.Category == cCategory)
                    {
                        l = category.Length;
                    }
                }
                foreach (var match in font.FontMetricData.Matches)
                {
                    if (Matches(match.Match, codepoint))
                    {
                        l = match.Length;
                    }
                }
                return l;
            }

            bool Matches(string match, int codepoint)
            {
                if (match.Length == 1)
                {
                    return match[0] == codepoint;
                }
                // state = 0: looking for a match
                // state = 1: looking for a dash AND the first character matched the range
                // state = 2: looking for a last character of range AND the first character matched the range
                int state = 0;
                for (int i = 0; i < match.Length; i++)
                {
                    var c = match[i];
                    if (c == '-')
                    {
                        if (state == 1)
                            state = 2;
                    }
                    // Check if the current character is a high surrogate
                    else
                    {
                        int matchCodepoint = c;
                        if (char.IsHighSurrogate(c) && i + 1 < match.Length && char.IsLowSurrogate(match[i + 1]))
                        {
                            // Combine the high and low surrogates to get the code point
                            matchCodepoint = char.ConvertToUtf32(match, i);
                            // Skip the next character as we have processed a surrogate pair
                            i++;
                        }
                        if (state == 0 || state == 1)
                        {
                            if (matchCodepoint == codepoint)
                            {
                                return true;
                            }
                            state = matchCodepoint < codepoint ? 1 : 0;
                        }
                        else if (state == 2)
                        {
                            if (matchCodepoint >= codepoint)
                            {
                                return true;
                            }
                            state = 0;
                        }
                    }
                }
                return false;
            }
        }

        private struct ExcelFontMatch
        {
            public FontSizeEstimationData.FontMetricData FontMetricData;
            public float Multiplier;
        }

        private ExcelFontMatch Find(string fontName, bool bold, bool italic)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var style = FontStyle.Regular
                | (bold ? FontStyle.Bold : 0)
                | (italic ? FontStyle.Italic : 0);
#pragma warning restore CA1416 // Validate platform compatibility
            return Find(fontName, style, true);
        }

        private ExcelFontMatch Find(string fontName, FontStyle style, bool useDefault)
        {
            foreach (var record in _fontData.Records)
            {
                if (record.Data is FontSizeEstimationData.FontMetricData fontMetricData && string.Equals(fontMetricData.Name, fontName, System.StringComparison.InvariantCultureIgnoreCase) && fontMetricData.Style == style)
                {
                    return new ExcelFontMatch()
                    {
                        FontMetricData = fontMetricData,
                        Multiplier = 1f,
                    };
                }
                else if (record.Data is FontSizeEstimationData.FontRedirectData fontRedirectData && string.Equals(fontRedirectData.Name, fontName, System.StringComparison.InvariantCultureIgnoreCase) && fontRedirectData.Style == style)
                {
                    var match = Find(fontRedirectData.Redirect, fontRedirectData.RedirectStyle, false);
                    if (match.FontMetricData != null)
                    {
                        return new ExcelFontMatch()
                        {
                            FontMetricData = match.FontMetricData,
                            Multiplier = match.Multiplier * fontRedirectData.Multiplier,
                        };
                    }
                }
            }
#pragma warning disable CA1416 // Validate platform compatibility
            if ((style & FontStyle.Italic) != 0)
            {
                return Find(fontName, style & ~FontStyle.Italic, false);
            }
            else if ((style & FontStyle.Bold) != 0)
            {
                return Find(fontName, style & ~FontStyle.Bold, false);
            }
#pragma warning restore CA1416 // Validate platform compatibility
            else if (useDefault)
            {
                foreach (var record in _fontData.Records)
                {
                    if (record.Data is FontSizeEstimationData.FontDefaultData fontDefaultData)
                    {
                        return Find(fontDefaultData.Name, style, false);
                    }
                }
            }
            return default;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
