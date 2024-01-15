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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace OfficeOpenXml.AutoFit
{
    public static class FontMeasurer
    {
        /// <summary>
        /// Generates a <see cref="FontSizeEstimationData"/> object containing font metrics for all installed fonts.
        /// Also calculates a global multiplier for bold fonts based on the difference in width between regular and bold fonts.
        /// Character widths are measured for the ASCII character set; other characters are measured as dashes.
        /// </summary>
        /// <param name="familyNamePredicate">Predicate to enable/disable generation of data for specific family names.</param>
        /// <param name="defaultFamilyName">Indicates which font's metrics should be used when no match can be found.</param>
#if NET6_0_OR_GREATER
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
        public static FontSizeEstimationData GenerateFontMetrics(Func<string, bool> familyNamePredicate = null, string defaultFamilyName = null)
        {
            var b = new Bitmap(1, 1);
            var g = Graphics.FromImage(b);
            Font font = null;
            Font fontBold = null;

            try
            {
                var records = new List<FontSizeEstimationData.Record>();
                foreach (FontFamily fontFamily in FontFamily.Families)
                {
                    if (familyNamePredicate != null && !familyNamePredicate(fontFamily.Name))
                        continue;
                    font = new Font(fontFamily.Name, 12f, FontStyle.Regular);
                    var padding = g.MeasureString("99", font).Width;
                    var fontHeight = RoundTo0125(g.MeasureString("Qj", font).Height);
                    var paddingWidth = RoundTo0125(padding - (g.MeasureString("999", font).Width - padding) * 2f);
                    var dashWidth = RoundTo0125(g.MeasureString("9-9", font).Width - padding);
                    var unmatchedWidth = dashWidth;
                    var spaceWidth = RoundTo0125(g.MeasureString("9 9", font).Width - padding);
                    var uppercaseWidth = RoundTo0125((g.MeasureString("9ABCDEFGHIJKLMNOPQRSTUVWXYZ9", font).Width - padding) / 26f);
                    var lowercaseWidth = RoundTo0125((g.MeasureString("9abcdefghijklmnopqrstuvwxyz9", font).Width - padding) / 26f);
                    var digitWidth = RoundTo0125((g.MeasureString("901234567899", font).Width - padding) / 10f);

                    // calculate categories
                    var categories = new List<FontSizeEstimationData.CategoryData>()
                    {
                        new FontSizeEstimationData.CategoryData()
                        {
                            Category = UnicodeCategory.UppercaseLetter,
                            Length = uppercaseWidth,
                        },
                        new FontSizeEstimationData.CategoryData()
                        {
                            Category = UnicodeCategory.LowercaseLetter,
                            Length = lowercaseWidth,
                        },
                        new FontSizeEstimationData.CategoryData()
                        {
                            Category = UnicodeCategory.DecimalDigitNumber,
                            Length = digitWidth,
                        },
                        new FontSizeEstimationData.CategoryData()
                        {
                            Category = UnicodeCategory.SpaceSeparator,
                            Length = spaceWidth,
                        },
                    };
                    // remove all categories that match the default width
                    categories.RemoveAll(x => x.Length == unmatchedWidth);

                    var matchDictionary = new Dictionary<int, List<char>>();
                    for (char c = '!'; c <= '~'; c++)
                    {
                        if (c == '-')
                        {
                            continue;
                        }
                        var widthF = RoundTo0125(g.MeasureString("9" + c + "9", font).Width - padding);
                        var width = (int)(widthF * 8f);
                        if (width > 255) width = 255;
                        // see if the character matches the default width based on the category or the unmatched width if no categories match
                        var defaultWidth = categories.FirstOrDefault(x => x.Category == CharUnicodeInfo.GetUnicodeCategory(c))?.Length ?? unmatchedWidth;
                        if (widthF == defaultWidth)
                            continue;
                        // otherwise, add it to the list of characters
                        if (!matchDictionary.TryGetValue(width, out var list))
                        {
                            list = new List<char>() { c };
                            matchDictionary[width] = list;
                        }
                        else
                        {
                            list.Add(c);
                        }
                    }
                    var matches = new List<FontSizeEstimationData.MatchData>();
                    foreach (var matchData in matchDictionary)
                    {
                        matches.Add(new FontSizeEstimationData.MatchData()
                        {
                            Match = CreateMatchString(matchData.Value),
                            Length = matchData.Key / 8f,
                        });
                    }
                    var fontMetricData = new FontSizeEstimationData.FontMetricData
                    {
                        Name = fontFamily.Name,
                        Style = FontStyle.Regular,
                        Dash = dashWidth,
                        Unmatched = dashWidth,
                        Padding = paddingWidth,
                        Height = fontHeight,
                        Categories = categories,
                        Matches = matches,
                    };
                    records.Add(new FontSizeEstimationData.Record() { Type = FontSizeEstimationData.FontMetricData.TYPE, Data = fontMetricData });
                    font.Dispose();
                    font = null;

                    fontBold = new Font(fontFamily.Name, 12f, FontStyle.Bold);
                    var paddingBold = g.MeasureString("99", fontBold).Width;
                    var dashWidthBold = RoundTo0125(g.MeasureString("9-9", fontBold).Width - paddingBold);
                    var spaceWidthBold = RoundTo0125((g.MeasureString("9 9", fontBold).Width - paddingBold));
                    var uppercaseWidthBold = RoundTo0125((g.MeasureString("9ABCDEFGHIJKLMNOPQRSTUVWXYZ9", fontBold).Width - paddingBold) / 26f);
                    var lowercaseWidthBold = RoundTo0125((g.MeasureString("9abcdefghijklmnopqrstuvwxyz9", fontBold).Width - paddingBold) / 26f);
                    var digitWidthBold = RoundTo0125((g.MeasureString("901234567899", fontBold).Width - paddingBold) / 10f);
                    fontBold.Dispose();
                    fontBold = null;

                    var totalRegular = dashWidth + uppercaseWidth + lowercaseWidth + digitWidth + spaceWidth;
                    var totalBold = dashWidthBold + uppercaseWidthBold + lowercaseWidthBold + digitWidthBold + spaceWidthBold;
                    var multiplier = totalBold / totalRegular;

                    var fontRedirectData = new FontSizeEstimationData.FontRedirectData()
                    {
                        Name = fontFamily.Name,
                        Style = FontStyle.Bold,
                        Redirect = fontFamily.Name,
                        RedirectStyle = FontStyle.Regular,
                        Multiplier = multiplier,
                    };
                    records.Add(new FontSizeEstimationData.Record() { Type = FontSizeEstimationData.FontRedirectData.TYPE, Data = fontRedirectData });
                }
                if (defaultFamilyName != null)
                {
                    if (records.Any(x => x.Data is FontSizeEstimationData.FontMetricData metricData && metricData.Name == defaultFamilyName && metricData.Style == FontStyle.Regular))
                    {
                        var fontDefaultData = new FontSizeEstimationData.FontDefaultData()
                        {
                            Name = defaultFamilyName,
                        };
                        records.Add(new FontSizeEstimationData.Record() { Type = FontSizeEstimationData.FontDefaultData.TYPE, Data = fontDefaultData });
                    }
                    else
                    {
                        throw new ArgumentException($"Default font family '{defaultFamilyName}' not found.", nameof(defaultFamilyName));
                    }
                }
                var data = new FontSizeEstimationData()
                {
                    Format = "FSED",
                    Major = 1,
                    Minor = 0,
                    Records = records,
                };
                return data;
            }
            finally
            {
                fontBold?.Dispose();
                font?.Dispose();
                g.Dispose();
                b.Dispose();
            }
        }

        private static float RoundTo0125(float value)
        {
            return (float)Math.Round(value * 8f, 0) / 8f;
        }

        private static string CreateMatchString(List<char> chars)
        {
            var chars2 = new List<char>(chars.Count);
            var consecutive = 0;
            char last = ' ';
            for (int i = 0; i < chars.Count; i++)
            {
                var c = chars[i];
                if (consecutive == 0)
                {
                    consecutive = 1;
                    last = c;
                    chars2.Add(c);
                }
                else if (consecutive == 1)
                {
                    if (c == ++last)
                    {
                        consecutive = 2;
                    }
                    else
                    {
                        last = c;
                        chars2.Add(c);
                    }
                }
                else if (consecutive == 2)
                {
                    if (c == ++last)
                    {
                        consecutive = 3;
                        chars2.Add('-');
                    }
                    else
                    {
                        chars2.Add(--last);
                        last = c;
                        consecutive = 1;
                        chars2.Add(c);
                    }
                }
                else if (consecutive == 3)
                {
                    if (c != ++last)
                    {
                        chars2.Add(--last);
                        last = c;
                        consecutive = 1;
                        chars2.Add(c);
                    }
                }
            }
            if (consecutive > 1)
                chars2.Add(last);
            return new string(chars2.ToArray());
        }
    }
}
