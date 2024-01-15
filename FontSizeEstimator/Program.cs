using OfficeOpenXml.AutoFit;
using System.Text;
using System.Text.Json;

Console.WriteLine("Measuring installed fonts.");

var fontName = "Arial";

// generate metrics for all fonts
var data = FontMeasurer.GenerateFontMetrics(x =>
{
    var ret = true; // x == fontName;
    if (ret) 
        Console.WriteLine($"Processing font: {x}");
    return ret;
}, "Arial"); // default font of Arial

// write serialized to out.dat
var serialized = BinarySerializer.Serialize(data);
Console.WriteLine("Binary serialized data length: " + serialized.Length);
using var file = new FileStream("out.dat", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
file.Write(serialized);
file.Close();

// write json to out.json
var fontMetricDataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });
Console.WriteLine("Json serialized data length: " + fontMetricDataJson.Length);
using var file2 = new FileStream("out.json", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
file2.Write(Encoding.UTF8.GetBytes(fontMetricDataJson));
file2.Close();

//Console.WriteLine(fontMetricDataJson);

using var m1 = new WindowsFormsTextMeasurer();
using var m2 = new EstimationTextMeasurer(data);


Console.WriteLine($"Measuring 'Hello World' with WindowsFormsTextMeasurer: {m1.MeasureString("Hello World", fontName, 12, false, false, false, false)}"); 
Console.WriteLine($"Measuring 'Hello World' with EstimationTextMeasurer: {m2.MeasureString("Hello World", fontName, 12, false, false, false, false)}");
Console.WriteLine($"Measuring 'a' with WindowsFormsTextMeasurer: {m1.MeasureString("a", fontName, 12, false, false, false, false)}");
Console.WriteLine($"Measuring 'a' with EstimationTextMeasurer: {m2.MeasureString("a", fontName, 12, false, false, false, false)}");

Console.WriteLine("Bold tests");
Console.WriteLine($"Measuring 'Hello World' with WindowsFormsTextMeasurer: {m1.MeasureString("Hello World", fontName, 12, true, false, false, false)}");
Console.WriteLine($"Measuring 'Hello World' with EstimationTextMeasurer: {m2.MeasureString("Hello World", fontName, 12, true, false, false, false)}");

Console.WriteLine("Italic tests");
Console.WriteLine($"Measuring 'Hello World' with WindowsFormsTextMeasurer: {m1.MeasureString("Hello World", fontName, 12, false, true, false, false)}");
Console.WriteLine($"Measuring 'Hello World' with EstimationTextMeasurer: {m2.MeasureString("Hello World", fontName, 12, false, true, false, false)}");


Console.WriteLine($"Measuring 'The quick brown fox jumps over the lazy dog.' with WindowsFormsTextMeasurer: {m1.MeasureString("The quick brown fox jumps over the lazy dog.", fontName, 12, false, false, false, false)}");
Console.WriteLine($"Measuring 'The quick brown fox jumps over the lazy dog.' with EstimationTextMeasurer: {m2.MeasureString("The quick brown fox jumps over the lazy dog.", fontName, 12, false, false, false, false)}");