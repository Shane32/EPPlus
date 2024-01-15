# Font size estimation data file format

This document describes the file format used by the
font size estimation tool for use with the `AutoFit`
methods on Linux.

Each file contains a version number, followed by a
sequence of records.

Any record type not understood by the reader should
be ignored.  This allows for future extensions to
the file format.

Incrementing the major version number indicates that
the file format has changed in a way that is not
backwards-compatible.  Incrementing the minor version
number indicates that the file format has changed in
a way that is backwards-compatible.

As such, a reader should not attempt to read any file
with a major version number greater than the one it
is expecting.

Unless otherwise specified, these rules apply:
- The order of data records are arbitrary.
- The file does not contain padding.
- All integer and floating-point values are stored in
  little-endian format.
- All 16-bit, 32-bit and 64-bit integers are signed.
- All 8-bit integers are unsigned.
- All floating-point values are stored in IEEE 754 format.
- Fixed length strings are encoded in ASCII and must
  not contain control characters.
- Variable length strings are encoded in UTF-8.
- Character lengths are stored as 8-bit integers,
  representing 8 times the length of the specified
  character in 12 point font, measured in pixels.
- Character heights are stored as 16-bit integers,
  representing 8 times the height of the specified
  character in 12 point font, measured in pixels.
- Strings do not have null terminators.

## Header

The header is 8 bytes long.  It contains four characters
`FSED` followed by four decimal digits.

The first two digits are the major version number;
the second two digits are the minor version number.

For example, the following header represents version
1.0 of the file format:

```
FSED0100
```

## Body

The body is a sequence of records.  Each record
begins with a four-byte record type, followed by
a four-byte record length, followed by the record
data.  The record type is a four-character string.
The record length is a 32-bit integer.

Record types are case-sensitive and may appear
multiple times in the file.

## Record types

Below is a list of record types and their meanings.

### `FNT1` Font metric data

This record contains the font metric data for a
single font.  The record data contains:

1. A 8-bit integer indicating the number
   of bytes in the following string.

2. The font name, encoded in UTF-8.

3. The font style as a byte where bit 0 is bold,
   and bit 1 is italic.

4. Character length of code point U+002D (standard dash character).

5. Character length of unmatched characters.

6. Length of padding to add to each line.

7. Height of the font.

8. Quantity of categories listed below as an 8-bit integer.

9. For each category:

   a. An 8-bit integer representing a Unicode character category
	  number, as listed in System.Globalization.UnicodeCategory.
   b. Average character length for characters not matched below.

10. A list of character matches and lengths, sorted such that the
    last match has the highest priority:

    a. An 8-bit integer representing the number
       of bytes in the following string.

    b. A string encoded in UTF-8 representing a list of characters
       or character ranges that match.  A character range is
       indicated by a dash character.  For example, the string
       `A-Z` represents all uppercase letters, while the string
       `A-Z0-9#` represents all uppercase letters and digits
       and the hash character.

    c. The average character length.

For instance, the following record data represents the font `Arial`
(not bold/italic) with a single average size of 9.25px for all characters,
a padding dimension of 4px, and a line height of 20px:

```
{5}Arial{0}{73}{73}{32}{160}{0}
```

### `FNTR` Font redirect

This record contains an redirect for a font.  The record data contains:

1. A 8-bit integer indicating the number
   of bytes in the following string.

2. The font name, encoded in UTF-8.

3. The font style as a byte where bit 0 is bold,
   and bit 1 is italic.

4. A 8-bit integer indicating the number
   of bytes in the following string.

5. The redirect font name, encoded in UTF-8.

6. The redirect font style as a byte where bit 0 is bold,
   and bit 1 is italic.

7. A 16-bit integer that when divided by 8192 and then added to 1 represents
   a multiplier for the character length when this redirect is used.  A value
   of 0 indicates that the character length should not be modified.  A value of
   8192 indicates that the character length should be doubled.  A value of
   -4096 indicates that the character length should be halved.

This record may refer to a font that has or has not yet been defined.

For example:

```
{4}Sans{0}{5}Arial{0}{0}
```

This indicates that when the font 'Sans' (not bold/italic) is requested,
the font 'Arial' (not bold/italic) should be used instead, with no modification
to the length.

### `FNTD` Font default

This record contains the default font to use when no other font
is specified.  The record data contains:

1. A 8-bit integer indicating the number
   of bytes in the following string.

2. The font name, encoded in UTF-8.

This record may refer to a font that has or has not yet been defined.

For example, the following indicates to use the metrics for 'Arial'
when the requested font cannot be found:

```
{5}Arial
```

## Human-readable format

The file format may also be represented in a JSON-encoded human-readable format.
This format is provided for debugging purposes only.

The JSON format omits the file length and string length fields as they are not
applicable, stores character lengths as floating-point values in pixels,
and multipliers as the floating-point multiplier values.
The order of the data matches the order in the format listed above.

See the sample below for exact field names and types:

```json
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
        "multiplier": 1,
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
```

In order to convert a JSON file to the binary format listed above, these restrictions
on the JSON data apply:

- The `format` field must be `FSED`.
- The `major` field must be 1.
- The `minor` field must be an integer from 0 to 99.
- Font name and alias fields must not be longer than 255 bytes after encoding as UTF-8.
- Character match strings must not be longer than 255 bytes after encoding as UTF-8.
- Character length values must be between 0 and 31.875, rounded to the nearest 0.125.
- Character height values must be between 0 and 4095.875, rounded to the nearest 0.125.
- Multipliers must be between 0 and 4.9999.
- Category values must be between 0 and 255 (only 0-29 are currently valid).
- Font style values must be between 0 and 255 (only 0-3 are currently valid).
- No additional properties may be present within the JSON structure.
- `rawData` fields must be a valid base-64 string, as defined in RFC 4648.

It is recommended that JSON readers tolerate comments and trailing commas within
the JSON structure.
