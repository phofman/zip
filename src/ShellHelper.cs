using System.Reflection;
using System.Threading;

namespace System.IO.Compression
{
    /// <summary>
    /// Helper class for some Shell32 operations.
    /// </summary>
    static class ShellHelper
    {
        /// <summary>
        /// Simple wrapper class for making reflection easier.
        /// </summary>
        public class ReflectionWrapper
        {
            private readonly object _o;

            /// <summary>
            /// Init constructor.
            /// </summary>
            public ReflectionWrapper(object o)
            {
                if (o == null)
                    throw new ArgumentNullException("o");

                _o = o;
            }

            /// <summary>
            /// Gets the COM type of the wrapped object.
            /// </summary>
            protected Type WrappedType
            {
                get { return _o.GetType(); }
            }

            /// <summary>
            /// Gets the wrapped object value.
            /// </summary>
            protected internal object WrappedObject
            {
                get { return _o; }
            }
        }

        public class Folder : ReflectionWrapper
        {
            public Folder(object o, string path)
                : base(o)
            {
                Path = path;
            }

            #region Properties

            public string Path
            {
                get;
                private set;
            }

            #endregion

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            public override string ToString()
            {
                return Path;
            }

            public FolderItems Items()
            {
                return new FolderItems(WrappedType.InvokeMember("Items", BindingFlags.InvokeMethod, null, WrappedObject, null));
            }

            public void Copy(ReflectionWrapper items, bool waitForCompletion, Action<Folder> completionHandler)
            {
                const int NoProgressDialog = 4;
                const int RespondYesToAllDialogs = 16;
                const int NoUiOnError = 1024;

                WrappedType.InvokeMember("CopyHere", BindingFlags.InvokeMethod, null, WrappedObject, new[] { items.WrappedObject, NoProgressDialog | RespondYesToAllDialogs | NoUiOnError });
                if (waitForCompletion)
                {
                    Wait();
                    if (completionHandler != null)
                    {
                        completionHandler(this);
                    }
                }
                else
                {
                    if (completionHandler != null)
                    {
                        var execAction = new Action<string>(WaitForCompletion);
                        execAction.BeginInvoke(Path, StartCopyMonitorCompleted, execAction);
                    }
                }
            }

            public void Wait()
            {
                WaitForCompletion(Path);
            }

            private void StartCopyMonitorCompleted(IAsyncResult ar)
            {
                // release async-call system resources:
                var action = (Action<string>) ar.AsyncState;
                action.EndInvoke(ar);
            }
        }

        public class FolderItems : ReflectionWrapper
        {
            public FolderItems(object o)
                : base(o)
            {
            }

            public int Count
            {
                get { return (int)WrappedType.InvokeMember("Count", BindingFlags.GetProperty, null, WrappedObject, null); }
            }

            public FolderItem this[int index]
            {
                get { return new FolderItem(WrappedType.InvokeMember("Item", BindingFlags.InvokeMethod, null, WrappedObject, new object[] { index })); }
            }
        }

        public class FolderItem : ReflectionWrapper
        {
            public FolderItem(object o)
                : base(o)
            {
            }

            public bool IsFolder
            {
                get { return (bool) WrappedType.InvokeMember("IsFolder", BindingFlags.GetProperty, null, WrappedObject, null); }
            }

            public string Name
            {
                get { return (string) WrappedType.InvokeMember("Name", BindingFlags.GetProperty, null, WrappedObject, null); }
                set { WrappedType.InvokeMember("Name", BindingFlags.SetProperty, null, WrappedObject, new object[] { value }); }
            }

            public long Size
            {
                get { return (int) WrappedType.InvokeMember("Size", BindingFlags.GetProperty, null, WrappedObject, null); }
            }

            public string Path
            {
                get { return (string) WrappedType.InvokeMember("Path", BindingFlags.GetProperty, null, WrappedObject, null); }
            }

            public Folder AsFolder
            {
                get
                {
                    if (IsFolder)
                    {
                        return new Folder(WrappedType.InvokeMember("GetFolder", BindingFlags.GetProperty, null, WrappedObject, null), Path);
                    }

                    return null;
                }
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            public override string ToString()
            {
                return Path;
            }

            public void Wait(string path)
            {
                WaitForCompletion(path);
            }
        }

        /// <summary>
        /// Gets the folder wrapper representation for specified path.
        /// </summary>
        public static Folder GetShell32Folder(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path) && !File.Exists(path))
                throw new ArgumentOutOfRangeException("path", "Requested path doesn't exist");

            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            Object shell = Activator.CreateInstance(shellAppType);
            return new Folder(shellAppType.InvokeMember("NameSpace", BindingFlags.InvokeMethod, null, shell, new object[] { path }), path);
        }

        private static void WaitForCompletion(string fileName)
        {
            Thread.Sleep(1000);
            while (File.Exists(fileName) && IsInUse(fileName))
            {
                Thread.Sleep(500);
            }
        }

        private static bool IsInUse(string filePath)
        {
            try
            {
                var file = File.OpenRead(filePath);
                file.Close();
                return false;
            }
            catch
            {
                return true;
            }
        }

    }
}
