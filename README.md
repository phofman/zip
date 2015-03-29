# CodeTitans ZipArchive Project
Library providing .NET 4.5 [ZipArchive](https://msdn.microsoft.com/en-us/library/system.io.compression.ziparchive(v=vs.110).aspx) API (System.IO.Compression) for older desktop .NET frameworks.

It uses build-in Win32 Shell API via reflection allowing .NET 2.0 applications to manipulate ZIP containers without any other dependencies. There are no 3rd party libraries, nor COM TypeLibraries embedded. It's as pure as few code files (using Windows internal components) that can be dropped directly into your project.

# License
It's licensed under MIT License, free for commercial and personal usage.

# Integration
Simply continue coding using [ZipArchive](https://msdn.microsoft.com/en-us/library/system.io.compression.ziparchive(v=vs.110).aspx) class in your .NET 4.5 application. Then for .NET 2.0 version of your application (that targets Windows XP or similar) simply include those few additional classes from this project.

# How it works
The initial idea was presented on [CodeProject.com](http://www.codeproject.com/Articles/12064/Compress-Zip-files-with-Windows-Shell-API-and-C). Gerald Gibson Jr was using Shell32 COM-wrapper class and used its API to force shell to copy specified file to/from the ZIP archive.

What I did? I removed the dependency to Shell32 library by using reflection. Don't worry, this shouldn't hit the performance much. It's just used in few places to communicate and pass parameters (paths and flags). The whole compression and decompression is executed on the native Windows Shell side, what is really fast. Finally, having a way to manipulate files and ZIP archive's internals I exposed it in the same way .NET 4.5 does in [ZipArchive](https://msdn.microsoft.com/en-us/library/system.io.compression.ziparchive(v=vs.110).aspx)/[ZipArchiveEntry](https://msdn.microsoft.com/en-us/library/system.io.compression.ziparchiveentry(v=vs.110).aspx)/[ZipFile](https://msdn.microsoft.com/en-us/library/system.io.compression.zipfile(v=vs.110).aspx) classes.

# Diffircences
There are few quirks and differences, that you should be familiar with:
 * if the whole process takes more than few seconds, Windows will show a progress UI and give the user ability to cancel ZIP manipulation
 * the whole compression/decompression is executed by the background thread in Windows Shell and there was no easy way to determin, when it completes
 * thread sleeps and polling are used to determin, when the processing did finished (I know it's bad, but your application is not only processing ZIP files, right? it's only an extra feature used once for a while and 0.5 sec delay won't hurt you, like you compress some saves or exported materials; check [ShellHelper](/src/ShellHelper.cs).WaitForCompletion() for details)
 * deletion from an already existing ZIP archive is not fully supported

# Bugs
If you find any issues or misbehaviours, please let me know via [Issues](https://github.com/phofman/zip/issues) section.

# Requirements
Microsoft .NET Framework 2.0+

Windows XP/Vista/7/8

