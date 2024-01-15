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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Buffers.Binary;
#endif
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OfficeOpenXml.AutoFit
{
    public static class BinarySerializer
    {
        public static FontSizeEstimationData Deserialize(Stream stream)
        {
            var maxCount = long.MaxValue;

            var ret = new FontSizeEstimationData();
            ReadHeader(stream, ret, ref maxCount);

            ret.Records = new List<FontSizeEstimationData.Record>();
            while (true)
            {
                if (!TryReadAsciiString(stream, 4, ref maxCount, out var type))
                    break;
                if (!TryReadInt32LittleEndian(stream, ref maxCount, out var length) || length < 0)
                    break;
                var length64 = (long)length;
                var record = new FontSizeEstimationData.Record();
                record.Type = type;
                switch (type)
                {
                    case FontSizeEstimationData.FontMetricData.TYPE:
                        if (!TryReadFontMetricData(stream, ref length64, out var metricData))
                            return ret;
                        record.Data = metricData;
                        break;
                    case FontSizeEstimationData.FontRedirectData.TYPE:
                        if (!TryReadFontAliasData(stream, ref length64, out var aliasData))
                            return ret;
                        record.Data = aliasData;
                        break;
                    case FontSizeEstimationData.FontDefaultData.TYPE:
                        if (!TryReadFontDefaultData(stream, ref length64, out var defaultData))
                            return ret;
                        record.Data = defaultData;
                        break;
                    default:
                        var buf = new byte[length];
                        if (!TryReadExactly(stream, buf, 0, length, ref length64))
                            return ret;
                        record.RawData = buf;
                        break;
                }
                if (length64 > 0)
                {
                    if (!TryReadExactly(stream, new byte[length64], 0, (int)length64, ref length64))
                        return ret;
                }
                maxCount -= length;
                ret.Records.Add(record);
            }

            return ret;
        }

        private static bool TryReadFontAliasData(Stream stream, ref long maxCount, out FontSizeEstimationData.FontRedirectData data)
        {
            data = new FontSizeEstimationData.FontRedirectData();
            if (!TryReadUtf8String(stream, ref maxCount, out var name))
                return false;
            data.Name = name;
            if (!TryReadByte(stream, ref maxCount, out var b))
                return false;
            data.Style = (FontStyle)b;
            if (!TryReadUtf8String(stream, ref maxCount, out name))
                return false;
            data.Redirect = name;
            if (!TryReadByte(stream, ref maxCount, out b))
                return false;
            data.RedirectStyle = (FontStyle)b;
            if (!TryReadInt16LittleEndian(stream, ref maxCount, out short s))
                return false;
            data.Multiplier = s / 8192f + 1f;
            return true;
        }

        private static bool TryReadFontDefaultData(Stream stream, ref long maxCount, out FontSizeEstimationData.FontDefaultData data)
        {
            data = new FontSizeEstimationData.FontDefaultData();
            if (!TryReadUtf8String(stream, ref maxCount, out var name))
                return false;
            data.Name = name;
            return true;
        }

        private static bool TryReadFontMetricData(Stream stream, ref long maxCount, out FontSizeEstimationData.FontMetricData data)
        {
            data = new FontSizeEstimationData.FontMetricData();
            if (!TryReadUtf8String(stream, ref maxCount, out var name))
                return false;
            data.Name = name;
            if (!TryReadByte(stream, ref maxCount, out var b))
                return false;
            data.Style = (FontStyle)b;
            if (!TryReadLength(stream, ref maxCount, out var len))
                return false;
            data.Dash = len;
            if (!TryReadLength(stream, ref maxCount, out len))
                return false;
            data.Unmatched = len;
            if (!TryReadLength(stream, ref maxCount, out len))
                return false;
            data.Padding = len;
            if (!TryReadHeight(stream, ref maxCount, out var h))
                return false;
            data.Height = h;
            if (!TryReadByte(stream, ref maxCount, out var entries))
                return false;
            data.Categories = new List<FontSizeEstimationData.CategoryData>();
            for (int i = 0; i < entries; i++)
            {
                if (!TryReadByte(stream, ref maxCount, out var catNum))
                    return false;
                if (!TryReadLength(stream, ref maxCount, out len))
                    return false;
                data.Categories.Add(new FontSizeEstimationData.CategoryData()
                {
                    Category = (System.Globalization.UnicodeCategory)catNum,
                    Length = len,
                });
            }
            data.Matches = new List<FontSizeEstimationData.MatchData>();
            while (true)
            {
                if (!TryReadUtf8String(stream, ref maxCount, out name))
                    return true;
                if (!TryReadLength(stream, ref maxCount, out len))
                    return false;
                data.Matches.Add(new FontSizeEstimationData.MatchData()
                {
                    Match = name,
                    Length = len,
                });
            }
        }

        public static void Serialize(FontSizeEstimationData data, Stream stream)
        {
            Validate(data);

            // write header
            WriteAsciiString(stream, $"FSED{data.Major.ToString("00", CultureInfo.InvariantCulture)}{data.Minor.ToString("00", CultureInfo.InvariantCulture)}");
            
            if (data.Records == null)
                return;

            // write records
            var recordStream = new MemoryStream();
            foreach (var record in data.Records)
            {
                if (record == null)
                    continue;
                WriteAsciiString(stream, record.Type);
                if (record.RawData != null)
                {
                    WriteInt32LittleEndian(stream, record.RawData.Length);
                    stream.Write(record.RawData, 0, record.RawData.Length);
                    continue;
                }
                else if (record.Data is FontSizeEstimationData.FontMetricData metricData)
                {
                    WriteMetricData(recordStream, metricData);
                }
                else if (record.Data is FontSizeEstimationData.FontRedirectData aliasData)
                {
                    WriteUtf8String(recordStream, aliasData.Name);
                    recordStream.WriteByte((byte)aliasData.Style);
                    WriteUtf8String(recordStream, aliasData.Redirect);
                    recordStream.WriteByte((byte)aliasData.RedirectStyle);
                    WriteInt16LittleEndian(recordStream, checked((short)Math.Round((aliasData.Multiplier - 1f) * 8192d)));
                }
                else if (record.Data is FontSizeEstimationData.FontDefaultData defaultData)
                {
                    WriteUtf8String(recordStream, defaultData.Name);
                }
                recordStream.Position = 0;
                WriteInt32LittleEndian(stream, checked((int)recordStream.Length));
#if NET35
                var buf = new byte[4096];
                while (true)
                {
                    var read = recordStream.Read(buf, 0, 4096);
                    if (read == 0)
                        break;
                    stream.Write(buf, 0, read);
                }
#else
                recordStream.CopyTo(stream);
#endif
                recordStream.Position = 0;
                recordStream.SetLength(0);
            }
        }

        private static void WriteMetricData(Stream stream, FontSizeEstimationData.FontMetricData data)
        {
            WriteUtf8String(stream, data.Name);
            stream.WriteByte((byte)data.Style);
            WriteLength(stream, data.Dash);
            WriteLength(stream, data.Unmatched);
            WriteLength(stream, data.Padding);
            WriteHeight(stream, data.Height);
            if (data.Categories == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                int count = 0;
                foreach (var cat in data.Categories)
                    count++;
                stream.WriteByte(checked((byte)count));
                foreach (var cat in data.Categories)
                {
                    if (cat == null)
                        continue;
                    stream.WriteByte(checked((byte)cat.Category));
                    WriteLength(stream, cat.Length);
                }
            }
            if (data.Matches == null)
                return;
            foreach (var match in data.Matches)
            {
                WriteUtf8String(stream, match.Match);
                WriteLength(stream, match.Length);
            }
        }

        private static void WriteLength(Stream stream, float length)
        {
            var value = checked((byte)Math.Round(length * 8f, MidpointRounding.AwayFromZero));
            stream.WriteByte(value);
        }

        private static void WriteHeight(Stream stream, float length)
        {
            var value = checked((short)Math.Round(length * 8f, MidpointRounding.AwayFromZero));
            WriteInt16LittleEndian(stream, value);
        }

        private static void Validate(FontSizeEstimationData data)
        {
            if (data.Format != "FSED")
                throw new InvalidOperationException("Format must be FSED.");
            if (data.Major != 1)
                throw new InvalidOperationException("Major version must be 1.");
            if (data.Minor < 0 || data.Minor > 99)
                throw new InvalidOperationException("Minor version must be 0-99.");
            if (data.Records == null)
                return;
            foreach (var record in data.Records)
            {
                if (record == null)
                    continue;
                Validate(record);
            }
        }

        private static void Validate(FontSizeEstimationData.Record record)
        {
            if (record.Data != null && record.RawData != null)
                throw new InvalidOperationException("Cannot specify both Data and RawData in a record");
            if (record.Type == null || record.Type.Length != 4)
                throw new InvalidOperationException("Record type must be 4 characters long.");
            if (record.Type.Any(c => c < ' ' || c > '\u007F'))
                throw new InvalidOperationException("Record type must be ASCII characters.");
            if (record.Data is FontSizeEstimationData.FontMetricData fontMetricData)
            {
                Validate(fontMetricData);
            }
            else if (record.Data is FontSizeEstimationData.FontRedirectData fontAliasData)
            {
                ValidateString(fontAliasData.Name);
                if ((int)fontAliasData.Style < 0 || (int)fontAliasData.Style > 255)
                    throw new InvalidOperationException("Style must be 0-255.");
                ValidateString(fontAliasData.Redirect);
                if ((int)fontAliasData.RedirectStyle < 0 || (int)fontAliasData.RedirectStyle > 255)
                    throw new InvalidOperationException("Style must be 0-255.");
            }
            else if (record.Data is FontSizeEstimationData.FontDefaultData fontDefaultData)
            {
                ValidateString(fontDefaultData.Name);
            }
            else if (record.Data != null)
            {
                throw new InvalidOperationException("Unrecognized record data format.");
            }
        }

        private static void Validate(FontSizeEstimationData.FontMetricData data)
        {
            ValidateString(data.Name);
            if ((int)data.Style < 0 || (int)data.Style > 255)
                throw new InvalidOperationException("Style must be 0-255.");
            ValidateLength(data.Dash);
            ValidateLength(data.Unmatched);
            ValidateLength(data.Padding);
            ValidateHeight(data.Height);
            if (data.Categories != null)
            {
                if (data.Categories.Count > 255)
                    throw new InvalidOperationException("Category count must be under 256.");
                foreach (var category in data.Categories)
                {
                    if (category == null)
                        continue;
                    Validate(category);
                }
            }
            if (data.Matches != null)
            {
                foreach (var match in data.Matches)
                {
                    if (match == null)
                        continue;
                    Validate(match);
                }
            }
        }
        private static void Validate(FontSizeEstimationData.MatchData matchData)
        {
            ValidateString(matchData.Match);
            ValidateLength(matchData.Length);
        }
        private static void ValidateString(string str)
        {
            if (str == null)
                return;
            if (Encoding.UTF8.GetByteCount(str) > 255)
                throw new InvalidOperationException("String length cannot be over 255 bytes after encoding as UTF-8.");
        }
        private static void Validate(FontSizeEstimationData.CategoryData category)
        {
            if ((int)category.Category < 0 || (int)category.Category > 255)
            {
                throw new InvalidOperationException("Invalid unicode category.");
            }
        }
        private static void ValidateLength(float length)
        {
            if (length < 0 || length > 31.876f)
            {
                throw new InvalidOperationException("Length must be between 0 and 31.875.");
            }
        }
        private static void ValidateHeight(float length)
        {
            if (length < 0 || length > 4095.876f)
            {
                throw new InvalidOperationException("Height must be between 0 and 4095.875.");
            }
        }
        private static void ReadHeader(Stream stream, FontSizeEstimationData ret, ref long maxCount)
        {
            if (!TryReadAsciiString(stream, 8, ref maxCount, out var str))
                throw new EndOfStreamException();

            ret.Format = str.Substring(0, 4);
            if (ret.Format != "FSED")
                throw new FormatException("Missing FSED header.");

            if (!byte.TryParse(str.Substring(4, 2), out var major) || major != 1)
                throw new FormatException("Invalid major version.");
            ret.Major = major;

            if (!byte.TryParse(str.Substring(6, 2), out var minor) || minor < 0)
                throw new FormatException("Invalid minor version.");
            ret.Minor = minor;
        }

        public static FontSizeEstimationData Deserialize(byte[] bytes)
        {
            return Deserialize(new MemoryStream(bytes));
        }

        public static byte[] Serialize(FontSizeEstimationData data)
        {
            var stream = new MemoryStream();
            Serialize(data, stream);
            return stream.ToArray();
        }

        private static bool TryReadByte(Stream stream, ref long maxCount, out byte result)
        {
            if (maxCount == 0)
            {
                result = default;
                return false;
            }
            int b = stream.ReadByte();
            if (b == -1)
            {
                result = default;
                return false;
            }
            result = (byte)b;
            maxCount--;
            return true;
        }

        private static bool TryReadLength(Stream stream, ref long maxCount, out float length)
        {
            if (!TryReadByte(stream, ref maxCount, out var b))
            {
                length = 0f;
                return false;
            }
            length = b / 8.0f;
            return true;
        }

        private static bool TryReadHeight(Stream stream, ref long maxCount, out float length)
        {
            if (!TryReadInt16LittleEndian(stream, ref maxCount, out var v))
            {
                length = 0f;
                return false;
            }
            length = v / 8.0f;
            return true;
        }

        private static void WriteAsciiString(Stream stream, string str)
        {
            if (str == null || str.Length == 0)
                return;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buf = stackalloc byte[str.Length];
            Encoding.ASCII.GetBytes(str, buf);
            stream.Write(buf);
#else
            var data = Encoding.ASCII.GetBytes(str);
            stream.Write(data, 0, data.Length);
#endif
        }

        private static bool TryReadAsciiString(Stream stream, int length, ref long maxCount, out string value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buf = stackalloc byte[length];
            if (!TryReadExactly(stream, buf, ref maxCount))
#else
            var buf = new byte[length];
            if (!TryReadExactly(stream, buf, 0, length, ref maxCount))
#endif
            {
                value = null;
                return false;
            }
            value = Encoding.ASCII.GetString(buf);
            return true;
        }

        private static void WriteUtf8String(Stream stream, string str)
        {
            if (str == null || str.Length == 0)
            {
                stream.WriteByte((byte)0);
                return;
            }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            var len = Encoding.UTF8.GetByteCount(str);
            Span<byte> chars = stackalloc byte[len];
            Encoding.UTF8.GetBytes(str, chars);
            stream.WriteByte((byte)chars.Length);
            stream.Write(chars);
#else
            var chars = Encoding.UTF8.GetBytes(str);
            stream.WriteByte((byte)chars.Length);
            stream.Write(chars, 0, chars.Length);
#endif
        }

        private static bool TryReadUtf8String(Stream stream, ref long maxCount, out string value)
        {
            if (!TryReadByte(stream, ref maxCount, out byte length))
            {
                value = null;
                return false;
            }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buf = stackalloc byte[length];
            if (!TryReadExactly(stream, buf, ref maxCount))
#else
            var buf = new byte[length];
            if (!TryReadExactly(stream, buf, 0, length, ref maxCount))
#endif
            {
                value = null;
                return false;
            }
            value = Encoding.UTF8.GetString(buf);
            return true;
        }

        private static void WriteInt16LittleEndian(Stream stream, short value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buf = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(buf, value);
            stream.Write(buf);
#else
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            stream.Write(bytes, 0, 2);
#endif
        }

        private static bool TryReadInt16LittleEndian(Stream stream, ref long maxCount, out short value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[2];
            if (!TryReadExactly(stream, buffer, ref maxCount))
            {
                value = 0;
                return false;
            }
            value = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return true;
#else
            byte[] buffer = new byte[BitConverter.IsLittleEndian ? 2 : 4];
            if (!TryReadExactly(stream, buffer, 0, 2, ref maxCount))
            {
                value = 0;
                return false;
            }
            if (BitConverter.IsLittleEndian)
            {
                value = BitConverter.ToInt16(buffer, 0);
                return true;
            }
            buffer[2] = buffer[1];
            buffer[3] = buffer[0];
            value = BitConverter.ToInt16(buffer, 2);
            return true;
#endif
        }

        private static void WriteInt32LittleEndian(Stream stream, int value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buf = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(buf, value);
            stream.Write(buf);
#else
            var bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            stream.Write(bytes, 0, 4);
#endif
        }

        private static bool TryReadInt32LittleEndian(Stream stream, ref long maxCount, out int value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[4];
            if (!TryReadExactly(stream, buffer, ref maxCount))
            {
                value = 0;
                return false;
            }
            value = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return true;
#else
            byte[] buffer = new byte[BitConverter.IsLittleEndian ? 4 : 8];
            if (!TryReadExactly(stream, buffer, 0, 4, ref maxCount))
            {
                value = 0;
                return false;
            }
            if (BitConverter.IsLittleEndian)
            {
                value = BitConverter.ToInt32(buffer, 0);
                return true;
            }
            buffer[4] = buffer[3];
            buffer[5] = buffer[2];
            buffer[6] = buffer[1];
            buffer[7] = buffer[0];
            value = BitConverter.ToInt32(buffer, 4);
            return true;
#endif
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        private static bool TryReadExactly(Stream stream, Span<byte> buffer, ref long maxCount)
        {
            if (maxCount < 1)
            {
                return false;
            }
            var count = buffer.Length;
            if (count > maxCount)
            {
                TryReadExactly(stream, buffer.Slice(0, (int)maxCount), ref maxCount);
                return false;
            }
            while (buffer.Length > 0)
            {
                int read = stream.Read(buffer);
                if (read == 0)
                    return false;
                maxCount -= read;
                buffer = buffer.Slice(read);
            }
            return true;
        }

        private static bool TryReadExactly(Stream stream, byte[] buffer, int offset, int count, ref long maxCount)
        {
            return TryReadExactly(stream, new Span<byte>(buffer, offset, count), ref maxCount);
        }
#else
        private static bool TryReadExactly(Stream stream,
            byte[] buffer,
            int offset,
            int count,
            ref long maxCount)
        {
            if (maxCount < 1)
            {
                return false;
            }
            if (count > maxCount)
            {
                TryReadExactly(stream, buffer, offset, (int)maxCount, ref maxCount);
                return false;
            }
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read == 0)
                    return false;
                offset += read;
                count -= read;
                maxCount -= read;
            }
            return true;
        }
#endif
    }
}
