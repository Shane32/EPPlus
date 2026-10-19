using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.IO;

namespace EPPlusTest
{
    [TestClass]
    public class ExcelPackageTests
    {
        [TestMethod]
        public void ReadWorkbookWithZip64DataDescriptors()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Workbooks", "test-zip64-descriptor.xlsx");
            using (var stream = File.OpenRead(path))
            using (var package = new ExcelPackage(stream))
            {
                Assert.AreEqual(1, package.Workbook.Worksheets.Count);
                var worksheet = package.Workbook.Worksheets["Sheet1"];
                Assert.IsNotNull(worksheet);
                Assert.AreEqual("test", worksheet.Cells["A1"].Value);
                Assert.AreEqual("A1", worksheet.Dimension.Address);
            }
        }

        [TestMethod]
        public void SaveAndReloadWorkbookWithZip64DataDescriptors()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Workbooks", "test-zip64-descriptor.xlsx");
            using (var input = File.OpenRead(path))
            using (var output = new MemoryStream())
            {
                using (var package = new ExcelPackage(input))
                {
                    package.SaveAs(output);
                }
                output.Position = 0;
                using (var package = new ExcelPackage(output))
                {
                    Assert.AreEqual(1, package.Workbook.Worksheets.Count);
                    Assert.AreEqual("test", package.Workbook.Worksheets["Sheet1"].Cells["A1"].Value);
                }
                Assert.IsTrue(input.CanRead, "Loading the package must leave the caller's input stream open.");
            }
        }
    }
}
