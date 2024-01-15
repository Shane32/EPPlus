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
        /// <param name="fontName">The name of the font to measure.</param>
        /// <param name="size">The size of the font to measure.</param>
        /// <param name="bold">Whether the font is bold.</param>
        /// <param name="italic">Whether the font is italic.</param>
        /// <param name="underline">Whether the font is underlined.</param>
        /// <param name="strikeout">Whether the font is strikethrough.</param>
        /// <param name="maxWidth">Maximum width of the string in pixels, or -1 for no maximum.</param>
        /// <returns>
        /// This method returns a <see cref="SizeF"/> structure that represents the size in pixels
        /// of the text specified by the <paramref name="text"/> parameter as drawn
        /// with the specified font and style.
        /// </returns>
        SizeF MeasureString(string text, string fontName, float size, bool bold, bool italic, bool underline, bool strikeout, int maxWidth = -1);
    }
}
