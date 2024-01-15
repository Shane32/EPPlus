# EPPlus-LGPL

[![Nuget](https://img.shields.io/nuget/v/EPPlus-LGPL)](https://www.nuget.org/packages/EPPlus-LGPL)

EPPlus-LGPL is an unofficial EPPlus library which includes bug fixes beyond EPPlus 4.5.3.3
while retaining a LGPL license, such as support for .NET 8. (The official version of EPPlus
v5+ is not available for commercial use without acquiring a license.)

## .NET 6+ support on Linux

All features are available on Windows. However, System.Drawing.Common support on Linux is deprecated
on .NET 6 and removed from .NET 7 and .NET 8. This library currently relies on its support for the
following features:

* Adding images
* Setting fonts via the `Font` class

So long as you do not use these features, you can use this library on .NET 6+ on Linux. Note that
you can set fonts by setting the individual font properties such as `Name`, `Bold`, `Italic`, `Size`,
and so on.

To use these features on prior versions of .NET Core running on Linux, you will need to install the
`libgdiplus` package. For example, on Ubuntu, you can run `sudo apt install libgdiplus` to install it.

To use these features on .NET 6 running on Linux, you must add the below to your project file.

```xml
<ItemGroup>
  <RuntimeHostConfigurationOption Include="System.Drawing.EnableUnixSupport" Value="true" />
</ItemGroup>
```

For more information, visit https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only

Please note that the auto-sizing columns feature uses font metrics collected from a Windows PC, and
may not carry metrics for the fonts you use.

***

# About EPPlus

## Create advanced Excel spreadsheets using .NET, without the need of interop.

EPPlus is a .NET library that reads and writes Excel files using the Office Open XML format (xlsx). 
EPPlus has no dependencies other than .NET.
 
## EPPlus supports:
* Cell Ranges 
* Cell styling (Border, Color, Fill, Font, Number, Alignments) 
* Data validation 
* Conditional formatting 
* Charts 
* Pictures 
* Shapes 
* Comments 
* Tables 
* Pivot tables 
* Protection 
* Encryption 
* VBA 
* Formula calculation 
* Many more... 

## Overview

This project started with the source from ExcelPackage. It was a great project to start from.
It had the basic functionality needed to read and write a spreadsheet.

Advantages over other:
- EPPlus uses dictionaries to access cell data, making performance a lot better.
- Complete integration with .NET 

## New features in version 4.5

* .NET Core support
* Sparklines
* Sort method added to ExcelRange
* Bug fixes and minor changes, see below and visit https://github.com/JanKallman/EPPlus for tutorials, samples and the latest information

Important Notes:
- Breaking change in .NET Core: The Worksheets collection will be zero based as default.
- This can be altered by setting the ExcelPackage.Compatibility.IsWorksheets1Based to true.
- .NET Core will have this property set to false, and .Net 3.5 and .Net 4 version will have this property set to true for backward compatibility reasons.
- This property can also be set via the appsettings.json file in .Net Core or the app.config file. See sample project for examples!

.NET Core uses a preview of System.Drawing.Common, so be aware of that. We will update it as Microsoft releases newer versions.
System.Drawing.Common requires libgdiplus to be installed on non-Windows operating systems.
Use your favorite package manager to install it.
For example:

Homebrew on MacOS:

```
brew install mono-libgdiplus
```

apt-get:

```
apt-get install libgdiplus
```

EPPlus-A .NET Spreadsheet API

## Changes since 4.0
4.5.3.11
* Added support for AutoFit on .NET 6+ on Linux using font metrics collected from a Windows PC

4.5.3.10
* Added additional compliation targets including .NET 6 and .NET 8; reduced dependencies

4.5.3.9
* Fixed recalculation under .NET Framework; support Source Link debugging

4.5.3.8
* Fixed the bug of address of ExcelNamedRange in ExcelNamedRangeCollection and Cell of ExcelSparkline in ExcelNamedRangeCollection when inserting row or column in ExcelWorksheet.

4.5.3.7
* Modify ExcelSparkline to support named data range.

4.5.3.3
* Support for .NET Standard 2.1.

4.5.3.2
* Added a target build for .NET Core 2.1 (netcoreapp2.1) with System.Drawing.Common 4.6.0-preview6.19303.8 
* Fixed Text property with short date format
* Fixed problem with defined names containing backslash 
* More bugfixes, see https://github.com/JanKallman/EPPlus/commits/master

4.5.3.1
* Fixed Lookup function ignoring result vector.
* Fixed address validation.

4.5.3
* Upgraded System.Drawing.Common for .NET Core to 4.5.1
* Enabled worksheetcharts to use a pivottable as source by adding a pivotTableSource parameter to the AddChart method of the Worksheets collection
* Pmt function
* And lots of bugfixes, see https://github.com/JanKallman/EPPlus/commits/master
      
4.5.2.1
* Upgraded System.Drawing.Common for .NET Core to 4.5.0
* Fixed problem with Apostrophe in worksheet name

4.5.2
* Upgraded System.Drawing.Common to 4.5.0-rc1
* Optimized image handling
* External Streams are not closed when disposing the package
* Fixed issue with Floor and Celing functions
* And more bugfixes, see https://github.com/JanKallman/EPPlus/commits/master

4.5.1
* Added web sample for .NET Core from Vahid Nasiri
* Added sample sparkline sample to sample project
* Fixed a few problems related to .NET Core on Mac

4.5.0.3
* Fix for compound documents (VBA and Encryption).
* Fix for Excel 2010 sha1 hashed agile encryption.
* Upgraded System.Drawing.Common to 4.5.0-preview1-26216-02
* Also see https://github.com/JanKallman/EPPlus/commits/master

4.5.0.2 rc
* Merge in e few pull requests and fixed a few issues. See https://github.com/JanKallman/EPPlus/commits/master


4.5.0.1 Beta 2
* Added sparkline support.
* Switched targetframework from netcoreapp2.0 to netstandardapp2.0
* Replaced CoreCompat.System.Drawing.v2 with System.Drawing.Common
* Fixed a few issues. See https://github.com/JanKallman/EPPlus/commits/master

4.5.0.0 Beta 1
* .Net Core support.
* Added ExcelPackage.Compatibility.IsWorksheets1Based to remove inconsistent 1 base collection on the worksheets collection.
Note: .Net Core will have this property set to false, and .Net 3.5 and .Net 4 version will have this property set to true for backward compatibility reasons.
This property can be set via the appsettings.json file in .Net Core or the app.config file. See sample project for examples.
* RoundedCorners property Add to ExcelChart
* DataTable propery Added  to ExcelPlotArea for charts
* Sort method added on ExcelRange
* Added functions NETWORKDAYS.INTL and NETWORKDAYS.
* And a lot of bug fixes. See https://github.com/JanKallman/EPPlus/commits/master

4.1.1
* Fix VBA bug in Excel 2016 - 1708 and later

4.1
* Added functions Rank, Rank.eq, Rank.avg and Search
* Applied a whole bunch of pull requests...
* Performance and memory usage tweeks
* Ability to set and retrieve 'custom' extended application propeties.
* Added style QuotePrefix
* Added support for MajorTimeUnit and MinorTimeUnit to chart axes
* Added GapWidth Property to BarChart and Gapwidth.
* Added Fill and Border properties to ChartSerie.
* Added support for MajorTimeUnit and MinorTimeUnit to chart axes
* Insert/delete row/column now shifts named ranges, comments, tables and pivottables.
* And fixed a lot of issues. See http://epplus.codeplex.com/SourceControl/list/changesets for more details

4.0.5 Fixes
* Switched to Visual Studio 2015 for code and sample projects.
* Added LineColor, MarkerSize, LineWidth and MarkerLineColor properties to line charts
* Added LineEnd properties to shapes
* Added functions Value, DateValue, TimeValue
* Removed WPF depedency.
* And fixed a lot of issues. See http://epplus.codeplex.com/SourceControl/list/changesets for more details

4.0.4 Fixes
* Added functions Daverage, Dvar Dvarp, DMax, DMin DSum,  DGet, DCount and DCountA 
* Exposed the formula parser logging functionality via FormulaParserManager.
* And fixed a lot of issues. See http://epplus.codeplex.com/SourceControl/list/changesets for more details

4.0.3 Fixes
* Added compilation directive for MONO (Thanks Danny)
* Added functions IfError, Char, Error.Type, Degrees, Fixed, IsNonText, IfNa and SumIfs
* And fixed a lot of issues. See http://epplus.codeplex.com/SourceControl/list/changesets for more details

4.0.2 Fixes
* Fixes a whole bunch of bugs related to the cell store (Worksheet.InsertColumn, Worksheet.InsertRow, Worksheet.DeleteColumn, Worksheet.DeleteRow, Range.Copy, Range.Clear)
* Added functions Acos, Acosh, Asinh, Atanh, Atan, CountBlank, CountIfs, Mina, Offset, Median, Hyperlink, Rept
* Fix for reading Excel comment content from the t-element.
* Fix to make Range.LoadFromCollection work better with inheritence
* And alot of other small fixes

4.0.1 Fixes
* VBA unreadable content
* Fixed a few issues with InsertRow and DeleteRow
* Fixed bug in Average and AverageA 
* Handling of Div/0 in functions
* Fixed VBA CodeModule error when copying a worksheet.
* Value decoding when reading str element for cell value.
* Better exception when accessing a worksheet out of range in the Excelworksheets indexer.
* Added Small and Large function to formula parser. Performance fix when encountering an unknown function.
* Fixed handling strings in formulas
* Calculate hangs if formula start with a parenthes.
* Worksheet.Dimension returned an invalid range in some cases.
* Rowheight was wrong in some cases.
* ExcelSeries.Header had an incorrect validation check.

## New features in 4.0

Replaced Packaging API with DotNetZip
* This will remove any problems with Isolated Storage and enable multithreading


New Cell store
* Less memory consumption
* Insert columns (not on the range level)
* Faster row inserts,

Formula Parser
* Calculates all formulas in a workbook, a worksheet or in a specified range
* 100+ functions implemented
* Access via Calculate methods on Workbook, Worksheet and Range objects.
* Add custom/missing Excel functions via Workbook. FormulaParserManager.
* Samples added to the EPPlusSamples project.

The formula parser does not support Array Formulas
* Intersect operator (Space)
* References to external workbooks
* And probably a whole lot of other stuff as well :)

Performance
*Of course the performance of the formula parser is nowhere near Excels. Our focus has been functionality.

Agile Encryption (Office 2012-)
* Support for newer type of encryption.

Minor new features
* Chart worksheets
* New Chart Types Bubblecharts
* Radar Charts
* Area Charts
* And lots of bug fixes...

Beta 2 Changes
* Fixed bug when using RepeatColumns & RepeatRows at the same time.
* VBA project will be left untouched if its not accessed.
* Fixed problem with strings on save.
* Added locks to the cell store for access by multiple threads.
* Implemented Indirect function
* Used DisplayNameAttribute to generate column headers from LoadFromCollection
* Rewrote ExcelRangeBase.Copy function. 
* Added caching to Save ZipStream for Cells and shared strings to speed up the Save method.
* Added Missing InsertColumn and DeleteColumn
* Added pull request to support Date1904 
* Added pull request ExcelWorksheet. LoadFromDataReader

Release Candidate changes
* Fixed some problems with Range.Copy Function
* InsertColumn and Delete column didn't work in some cases
* Chart.DisplayBlankAs had the wrong default type in Excel 2010+
* Datavalidation list overflow caused corruption of the package
* Fixed a few Calculation when referring ranges (for example If function)
* Added ChartAxis.DisplayUnit
* Fixed a bug related to shared formulas
* Named styles failed in some cases.
* Style.Indent got an invalid value in some cases.
* Fixed a problem with AutofitColumns method.
* Performance fix.
* A whole lot of other small fixes.

## License
The project is licensed under the GNU Library General Public License (LGPL). 

## Credits

Glory to Jehovah, Lord of Lords and King of Kings, creator of Heaven and Earth, who through his Son Jesus Christ,
has reedemed me to become a child of God. -Shane32
