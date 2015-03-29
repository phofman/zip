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

        #region Helper Methods

        /// <summary>
        /// Gets the file from the 'Content' folder of test input data.
        /// </summary>
        private string Content(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return ContentFolder;

            return Path.Combine(ContentFolder, fileName);
        }

        /// <summary>
        /// Gets the name for produced output, by concatenating paths.
        /// </summary>
        private string Output(string name, params string[] names)
        {
            var path = name.StartsWith(OutputFolder) ? name : Path.Combine(OutputFolder, name);

            foreach (var n in names)
            {
                path = Path.Combine(path, n);
            }
            return path;
        }

        /// <summary>
        /// Removes specified file or folder.
        /// </summary>
        private void Delete(string folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        /// <summary>
        /// Gets all files from specified directory (including all subdirectories).
        /// </summary>
        private string[] Files(string folder, string pattern = "*", bool updatePaths = false)
        {
            var result = Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);

            if (updatePaths)
            {
                int length = !string.IsNullOrEmpty(folder) ? folder.Length : 0;

                if (length > 0 && folder != null && folder[length - 1] != Path.AltDirectorySeparatorChar && folder[length - 1] != Path.DirectorySeparatorChar)
                    length++;

                // remove the 'folder' prefix from each path:
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = result[i].Substring(length);
                }
            }

            return result;
        }

        #endregion

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
                    Assert.IsNotNull(archive.Entries);
                    Assert.AreEqual(0, archive.Entries.Count);
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

            Delete(targetFolder);

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

            Delete(targetFolder);

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    Assert.IsNotNull(archive.Entries);
                    Assert.AreEqual(0, archive.Entries.Count);

                    archive.CreateEntryFromFile(Content("app.cfg"), entryName1);
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(entryName2);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.WriteLine("Information about author of this package.");
                        writer.WriteLine("=========================================");
                    }

                    Assert.IsNotNull(archive.Entries);
                    Assert.AreEqual(2, archive.Entries.Count);

                    archive.ExtractToDirectory(targetFolder);
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
            Assert.AreEqual(2, Files(targetFolder).Length, "Invalid number of extracted files");
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

            Delete(targetFolder);

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

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(targetFolder);
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
            Assert.IsTrue(File.Exists(Output(targetFolder, entryName1)), "Missing the extracted file");
            Assert.IsTrue(File.Exists(Output(targetFolder, entryName2)), "Missing the extracted file");
            Assert.AreEqual(2, Files(targetFolder).Length, "Invalid number of extracted files");
        }

        [TestMethod]
        public void EnumerateContent()
        {
            var inputTarget = Content("few_empty.zip");

            using (var zipToOpen = new FileStream(inputTarget, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    Assert.IsNotNull(archive.Entries);
                    Assert.IsTrue(archive.Entries.Count > 0, "Invalid number of items inside the archive found");

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
            var targetFolder = Output("zip-extract4");
            var inputTarget = Content("few_content.zip");
            int entriesCount;

            Delete(targetFolder);

            using (var zipToOpen = new FileStream(inputTarget, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    Assert.IsNotNull(archive.Entries);
                    Assert.IsTrue(archive.Entries.Count > 0, "Invalid number of items inside the archive found");

                    entriesCount = archive.Entries.Count;

                    foreach (var entry in archive.Entries)
                    {
                        Assert.AreNotEqual(0, entry.Length, "Expected all files to be empty!");
                        entry.ExtractToFile(Output(targetFolder, entry.FullName + ".xyz"));
                    }
                }
            }

            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
            Assert.AreEqual(entriesCount, Files(targetFolder, "*.xyz").Length, "Invalid number of extracted files");
        }

        [TestMethod]
        public void CreateZipFileWithStrangelyNamedContent()
        {
            var entryName1 = "docs and resources/entry.txt";
            var entryName2 = "credits.txt";
            var entryName3 = "readme.txt";
            var entryName4 = "polish_żółty.txt";

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.CreateEntry(entryName1);
                    archive.CreateEntry(entryName2);
                    archive.CreateEntry(entryName3);
                    archive.CreateEntry(entryName4);
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
        }

        [TestMethod]
        public void CreateZipFileAndDeleteSomeDynamicContent()
        {
            var entryName1 = "docs and resources/entry.txt";
            var entryName2 = "credits.txt";
            var entryName3 = "readme.txt";
            var entryName4 = "polish_żółty.txt";

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    Assert.IsNotNull(archive.Entries);
                    Assert.AreEqual(0, archive.Entries.Count, "Invalid number of items inside the empty archive found");

                    var item1 = archive.CreateEntry(entryName1);
                    archive.CreateEntry(entryName2);
                    archive.CreateEntry(entryName3);
                    var item2 = archive.CreateEntry(entryName4);

                    Assert.AreEqual(4, archive.Entries.Count, "Invalid number of items inside the archive found");

                    item1.Delete();
                    item2.Delete();

                    Assert.AreEqual(2, archive.Entries.Count, "Invalid number of items inside the archive found");
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
        }

        [TestMethod]
        public void CreateZipFileAndDeleteAllThenAddSomeContentAgain()
        {
            var entryName1 = "docs and resources/entry.txt";
            var entryName2 = "credits.txt";
            var entryName3 = "readme.txt";
            var entryName4 = "polish_żółty.txt";
            var entryName5 = "t1.txt";
            var entryName6 = "t2.txt";

            using (var zipToOpen = new FileStream(InputZipFileName, FileMode.OpenOrCreate))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    Assert.IsNotNull(archive.Entries);
                    Assert.AreEqual(0, archive.Entries.Count, "Invalid number of items inside the empty archive found");

                    // add items:
                    var item1 = archive.CreateEntry(entryName1);
                    var item2 = archive.CreateEntry(entryName2);
                    var item3 = archive.CreateEntry(entryName3);
                    var item4 = archive.CreateEntry(entryName4);

                    Assert.AreEqual(4, archive.Entries.Count, "Invalid number of items inside the archive found");

                    // remove all:
                    item1.Delete();
                    item2.Delete();
                    item3.Delete();
                    item4.Delete();

                    Assert.AreEqual(0, archive.Entries.Count, "Invalid number of items inside the empty archive found");

                    // add agian:
                    archive.CreateEntry(entryName5);
                    archive.CreateEntry(entryName6);

                    Assert.AreEqual(2, archive.Entries.Count, "Invalid number of items inside the archive found");
                }
            }

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
        }

        [TestMethod]
        public void CreateZipFileFromFolder()
        {
            ZipFile.CreateFromDirectory(Content(null), InputZipFileName, CompressionLevel.Optimal, true);

            Assert.IsTrue(File.Exists(InputZipFileName), "Missing the file, that should be created!");
            Assert.IsTrue(new FileInfo(InputZipFileName).Length > 0, "File shouldn't be empty!");
        }

        [TestMethod]
        public void ExtractZipFileIntoFolderFromPredefinedArchive()
        {
            var targetFolder = Output("zip-extract5");
            ZipFile.ExtractToDirectory(Content("few_content.zip"), targetFolder);

            Assert.IsTrue(Directory.Exists(targetFolder), "Missing the extraction area");
            Assert.IsTrue(Files(targetFolder).Length > 0, "Invalid number of extracted files");
        }
    }
}
