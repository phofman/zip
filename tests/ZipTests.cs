using System.IO;
using System.IO.Compression;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ZipTests
    {
        public ZipTests()
        {
            OutputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            InputZipFileName = Output("test.zip");
            ContentFolder = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(OutputFolder)), "TestContent");
        }

        #region Properties

        public string OutputFolder
        {
            get;
            private set;
        }

        public string InputZipFileName
        {
            get;
            private set;
        }

        public string ContentFolder
        {
            get;
            private set;
        }

        #endregion

        private string Content(string fileName)
        {
            return Path.Combine(ContentFolder, fileName);
        }

        private string Output(string name, params string[] names)
        {
            var path = name.StartsWith(OutputFolder) ? name : Path.Combine(OutputFolder, name);

            foreach (var n in names)
            {
                path = Path.Combine(path, n);
            }
            return path;
        }

        [TestInitialize]
        public void Setup()
        {
            if (File.Exists(InputZipFileName))
                File.Delete(InputZipFileName);
        }

        [TestMethod]
        public void CreateEmptyZipFile()
        {
            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
        }


        [TestMethod]
        public void CreateZipFileWithSomeStaticContent()
        {
            var targetFolder = Output("zip-extract");
            var entryName = "docs/NewEntry.txt";

            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(Content("app.cfg"), entryName);
                    archive.ExtractToDirectory(targetFolder);
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
            Assert.IsTrue(File.Exists(Output(targetFolder, entryName)), "Missing the extracted file");
        }

        [TestMethod]
        public void CreateZipFileWithOnTheFlyContent()
        {
            var targetFolder = Output("zip-extract2");
            var entryName1 = "docs/NewEntry.txt";
            var entryName2 = "credits.txt";

            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(Content("app.cfg"), entryName1);
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(entryName2);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.WriteLine("Information about author of this package.");
                        writer.WriteLine("=========================================");
                    }

                    archive.ExtractToDirectory(targetFolder);
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
        }

        [TestMethod]
        public void CreateZipFileMadeOfEmptyContent()
        {
            var entryName1 = "docs/NewEntry.txt";
            var entryName2 = "credits.txt";
            var entryName3 = "readme.txt";

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.CreateEntry(entryName1);
                    archive.CreateEntry(entryName2);
                    archive.CreateEntry(entryName3);
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
        }

        [TestMethod]
        public void CreateZipFileWithOnTheFlyContentAndExtractItSeparately()
        {
            var targetFolder = Output("zip-extract3");
            var entryName1 = "docs/NewEntry.txt";
            var entryName2 = "credits.txt";

            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(Content("app.cfg"), entryName1);
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(entryName2);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.WriteLine("Information about author of this package.");
                        writer.WriteLine("=========================================");
                    }
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
            Assert.IsTrue(File.Exists(Output(targetFolder, entryName1)), "Missing the extracted file");
            Assert.IsTrue(File.Exists(Output(targetFolder, entryName2)), "Missing the extracted file");
        }

        [TestMethod]
        public void EnumerateContent()
        {
            var inputTarget = Content("few_empty.zip");

            using (var zipToOpen = new FileStream(inputTarget, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    foreach (var entry in archive.Entries)
                    {
                        Assert.AreEqual(0, entry.Length, "Expected all files to be empty!");
                    }
                }
            }
        }

        [TestMethod]
        public void ExtractSingleFile()
        {
            var inputTarget = Content("few_content.zip");

            using (var zipToOpen = new FileStream(inputTarget, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    foreach (var entry in archive.Entries)
                    {
                        Assert.AreNotEqual(0, entry.Length, "Expected all files to be empty!");
                        entry.ExtractToFile(Path.Combine(@"D:\temp\xyz", entry.FullName + ".txt"));
                    }
                }
            }
        }
    }
}
