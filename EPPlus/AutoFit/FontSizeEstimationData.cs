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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
#if NETCOREAPP3_1_OR_GREATER
using System.Text.Json.Serialization;
#endif

namespace OfficeOpenXml.AutoFit
{
    public class FontSizeEstimationData
    {
#if NETCOREAPP3_1_OR_GREATER
        [JsonPropertyName("format")]
#endif
        public string Format { get; set; }
#if NETCOREAPP3_1_OR_GREATER
        [JsonPropertyName("major")]
#endif
        public int Major { get; set; }
#if NETCOREAPP3_1_OR_GREATER
        [JsonPropertyName("minor")]
#endif
        public int Minor { get; set; }

#if NETCOREAPP3_1_OR_GREATER
        [JsonPropertyName("records")]
#endif
        public List<Record> Records { get; set; }

#if NETCOREAPP3_1_OR_GREATER
        [JsonConverter(typeof(RecordJsonConverter))]
#endif
        public class Record
        {
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("type")]
#endif
            public string Type { get; set; }
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("data")]
#endif
            public object Data { get; set; }
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("rawData")]
#endif
            public byte[] RawData { get; set; }
        }

        public class FontMetricData
        {
            public const string TYPE = "FNT1";

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("name")]
#endif
            public string Name { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("style")]
#endif
            public FontStyle Style { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("dash")]
#endif
            public float Dash { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("unmatched")]
#endif
            public float Unmatched { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("padding")]
#endif
            public float Padding { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("height")]
#endif
            public float Height { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("categories")]
#endif
            public List<CategoryData> Categories { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("matches")]
#endif
            public List<MatchData> Matches { get; set; }
        }

        public class CategoryData
        {
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("category")]
#endif
            public UnicodeCategory Category { get; set; }
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("length")]
#endif
            public float Length { get; set; }
        }

        public class MatchData
        {
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("match")]
#endif
            public string Match { get; set; }
#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("length")]
#endif
            public float Length { get; set; }
        }

        public class FontRedirectData
        {
            public const string TYPE = "FNTR";

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("name")]
#endif
            public string Name { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("style")]
#endif
            public FontStyle Style { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("redirect")]
#endif
            public string Redirect { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("redirectStyle")]
#endif
            public FontStyle RedirectStyle { get; set; }

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("multiplier")]
#endif
            public float Multiplier { get; set; }
        }

        public class FontDefaultData
        {
            public const string TYPE = "FNTD";

#if NETCOREAPP3_1_OR_GREATER
            [JsonPropertyName("name")]
#endif
            public string Name { get; set; }
        }
    }
}
