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
#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OfficeOpenXml.AutoFit
{
    public class RecordJsonConverter : JsonConverter<FontSizeEstimationData.Record>
    {
        public override FontSizeEstimationData.Record Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                reader.Skip();
                return null;
            }

            var ret = new FontSizeEstimationData.Record();
            JsonElement data = default;
            bool dataSet = false;
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                var propName = reader.GetString();
                reader.Read();
                switch (propName)
                {
                    case "type":
                        ret.Type = reader.GetString();
                        break;
                    case "data":
                        switch (ret.Type)
                        {
                            case FontSizeEstimationData.FontMetricData.TYPE:
                                ret.Data = JsonSerializer.Deserialize<FontSizeEstimationData.FontMetricData>(ref reader, options);
                                break;
                            case FontSizeEstimationData.FontRedirectData.TYPE:
                                ret.Data = JsonSerializer.Deserialize<FontSizeEstimationData.FontRedirectData>(ref reader, options);
                                break;
                            case FontSizeEstimationData.FontDefaultData.TYPE:
                                ret.Data = JsonSerializer.Deserialize<FontSizeEstimationData.FontDefaultData>(ref reader, options);
                                break;
                            default:
                                data = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                                dataSet = true;
                                break;
                        }
                        break;
                    case "rawData":
                        ret.RawData = JsonSerializer.Deserialize<byte[]>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
            if (dataSet)
            {
                var data2 =
#if NET6_0_OR_GREATER
                    data;
#else
                    data.GetRawText();
#endif
                switch (ret.Type)
                {
                    case FontSizeEstimationData.FontMetricData.TYPE:
                        ret.Data = JsonSerializer.Deserialize<FontSizeEstimationData.FontMetricData>(data2, options);
                        break;
                    case FontSizeEstimationData.FontRedirectData.TYPE:
                        ret.Data = JsonSerializer.Deserialize<FontSizeEstimationData.FontRedirectData>(data2, options);
                        break;
                    case FontSizeEstimationData.FontDefaultData.TYPE:
                        ret.Data = JsonSerializer.Deserialize<FontSizeEstimationData.FontDefaultData>(data2, options);
                        break;
                    default:
                        ret.Data = data;
                        break;
                }
            }
            return ret;
        }

        public override void Write(Utf8JsonWriter writer, FontSizeEstimationData.Record value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", value.Type);
            if (value.RawData != null)
            {
                writer.WritePropertyName("rawData");
                JsonSerializer.Serialize(writer, value.RawData);
            }
            else if (value.Data == null)
            {
                writer.WriteString("data", (string)null);
            }
            else
            {
                writer.WritePropertyName("data");
                JsonSerializer.Serialize(writer, value.Data, options);
            }
            writer.WriteEndObject();
        }
    }
}
#endif