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
#pragma warning disable CA1416 // Validate platform compatibility
#if NET8_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml.AutoFit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace EPPlusTest.AutoFit
{
    [TestClass]
    public class AutoFitTests
    {
        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

        [TestMethod]
        public void WritesJson()
        {
            var data = GenerateSampleData();
            // assumes that JsonSerializer formats the same as the original
            var serialized = JsonSerializer.Serialize(data, _jsonOptions);
            Assert.AreEqual(JSON_SAMPLE_1, serialized);
        }

        [TestMethod]
        public void ReadsJson()
        {
            var data = JsonSerializer.Deserialize<FontSizeEstimationData>(JSON_SAMPLE_1, _jsonOptions);
            Verify(data);
        }

        [TestMethod]
        public void WritesBinary()
        {
            var data = GenerateSampleData();
            var serialized = BinarySerializer.Serialize(data);
            var base64 = Convert.ToBase64String(serialized);
            Assert.AreEqual(BINARY_SAMPLE_1, base64);
        }

        [TestMethod]
        public void ReadsBinary()
        {
            var data = BinarySerializer.Deserialize(Convert.FromBase64String(BINARY_SAMPLE_1));
            Verify(data);
        }

        private void Verify(FontSizeEstimationData data)
        {
            Assert.IsNotNull(data);
            Assert.AreEqual("FSED", data.Format);
            Assert.AreEqual(1, data.Major);
            Assert.AreEqual(0, data.Minor);
            Assert.IsNotNull(data.Records);
            Assert.AreEqual(4, data.Records.Count);
            Assert.AreEqual("FNT1", data.Records[0].Type);
            Assert.IsInstanceOfType(data.Records[0].Data, typeof(FontSizeEstimationData.FontMetricData));
            Assert.IsNull(data.Records[0].RawData);
            var metricData = (FontSizeEstimationData.FontMetricData)data.Records[0].Data;
            Assert.AreEqual("Arial", metricData.Name);
            Assert.AreEqual(FontStyle.Regular, metricData.Style);
            Assert.AreEqual(6.5f, metricData.Dash);
            Assert.AreEqual(8.25f, metricData.Unmatched);
            Assert.AreEqual(1f, metricData.Padding);
            Assert.AreEqual(20f, metricData.Height);
            Assert.IsNotNull(metricData.Categories);
            Assert.AreEqual(1, metricData.Categories.Count);
            Assert.AreEqual(UnicodeCategory.UppercaseLetter, metricData.Categories[0].Category);
            Assert.AreEqual(7.75f, metricData.Categories[0].Length);
            Assert.IsNotNull(metricData.Matches);
            Assert.AreEqual(1, metricData.Matches.Count);
            Assert.AreEqual("A-Z", metricData.Matches[0].Match);
            Assert.AreEqual(8.5f, metricData.Matches[0].Length);
            Assert.AreEqual("FNTR", data.Records[1].Type);
            Assert.IsInstanceOfType(data.Records[1].Data, typeof(FontSizeEstimationData.FontRedirectData));
            Assert.IsNull(data.Records[1].RawData);
            var redirectData = (FontSizeEstimationData.FontRedirectData)data.Records[1].Data;
            Assert.AreEqual("Arial", redirectData.Name);
            Assert.AreEqual(FontStyle.Regular, redirectData.Style);
            Assert.AreEqual("Sans", redirectData.Redirect);
            Assert.AreEqual(FontStyle.Regular, redirectData.RedirectStyle);
            Assert.AreEqual(1, redirectData.Multiplier);
            Assert.AreEqual("FNTD", data.Records[2].Type);
            Assert.IsInstanceOfType(data.Records[2].Data, typeof(FontSizeEstimationData.FontDefaultData));
            Assert.IsNull(data.Records[2].RawData);
            var defaultData = (FontSizeEstimationData.FontDefaultData)data.Records[2].Data;
            Assert.AreEqual("Arial", defaultData.Name);
            Assert.AreEqual("UNKN", data.Records[3].Type);
            Assert.IsNull(data.Records[3].Data);
            Assert.IsNotNull(data.Records[3].RawData);
            Assert.AreEqual("SGVsbG8sIHdvcmxkIQ==", Convert.ToBase64String(data.Records[3].RawData));
        }

        private const string BINARY_SAMPLE_1 = "RlNFRDAxMDBGTlQxFAAAAAVBcmlhbAA0QgigAAEAPgNBLVpERk5UUg8AAAAFQXJpYWwABFNhbnMAAABGTlREBgAAAAVBcmlhbFVOS04NAAAASGVsbG8sIHdvcmxkIQ==";

        private FontSizeEstimationData GenerateSampleData()
        {
            return new FontSizeEstimationData
            {
                Format = "FSED",
                Major = 1,
                Minor = 0,
                Records = new List<FontSizeEstimationData.Record>()
                {
                    new FontSizeEstimationData.Record()
                    {
                        Type = "FNT1",
                        Data = new FontSizeEstimationData.FontMetricData()
                        {
                            Name = "Arial",
                            Style = FontStyle.Regular,
                            Dash = 6.5f,
                            Unmatched = 8.25f,
                            Padding = 1f,
                            Height = 20f,
                            Categories = new List<FontSizeEstimationData.CategoryData>()
                            {
                                new FontSizeEstimationData.CategoryData()
                                {
                                    Category = UnicodeCategory.UppercaseLetter,
                                    Length = 7.75f,
                                },
                            },
                            Matches = new List<FontSizeEstimationData.MatchData>()
                            {
                                new FontSizeEstimationData.MatchData()
                                {
                                    Match = "A-Z",
                                    Length = 8.5f,
                                },
                            },
                        },
                    },
                    new FontSizeEstimationData.Record()
                    {
                        Type = "FNTR",
                        Data = new FontSizeEstimationData.FontRedirectData()
                        {
                            Name = "Arial",
                            Style = FontStyle.Regular,
                            Redirect = "Sans",
                            RedirectStyle = FontStyle.Regular,
                            Multiplier = 1,
                        },
                    },
                    new FontSizeEstimationData.Record()
                    {
                        Type = "FNTD",
                        Data = new FontSizeEstimationData.FontDefaultData()
                        {
                            Name = "Arial",
                        },
                    },
                    new FontSizeEstimationData.Record()
                    {
                        Type = "UNKN",
                        RawData = Encoding.UTF8.GetBytes("Hello, world!"),
                    },
                },
            };
        }

        private const string JSON_SAMPLE_1 = """
            {
              "format": "FSED",
              "major": 1,
              "minor": 0,
              "records": [
                {
                  "type": "FNT1",
                  "data": {
                    "name": "Arial",
                    "style": 0,
                    "dash": 6.5,
                    "unmatched": 8.25,
                    "padding": 1,
                    "height": 20,
                    "categories": [
                      {
                        "category": 0,
                        "length": 7.75
                      }
                    ],
                    "matches": [
                      {
                        "match": "A-Z",
                        "length": 8.5
                      }
                    ]
                  }
                },
                {
                  "type": "FNTR",
                  "data": {
                    "name": "Arial",
                    "style": 0,
                    "redirect": "Sans",
                    "redirectStyle": 0,
                    "multiplier": 1
                  }
                },
                {
                  "type": "FNTD",
                  "data": {
                    "name": "Arial"
                  }
                },
                {
                  "type": "UNKN",
                  "rawData": "SGVsbG8sIHdvcmxkIQ=="
                }
              ]
            }
            """;
    }
}
#endif