// https://stackoverflow.com/questions/666799/embedding-unmanaged-dll-into-a-managed-c-sharp-dll

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Security.Cryptography;

namespace RelhaxModpack
{
    /// <summary>
    /// A class used by managed classes to managed unmanaged DLLs.
    /// This will extract and load DLLs from embedded binary resources.
    /// 
    /// This can be used with pinvoke, as well as manually loading DLLs your own way. If you use pinvoke, you don't need to load the DLLs, just
    /// extract them. When the DLLs are extracted, the %PATH% environment variable is updated to point to the temporary folder.
    ///
    /// To Use
    /// <list type="">
    /// <item>Add all of the DLLs as binary file resources to the project Propeties. Double click Properties/Resources.resx,
    /// Add Resource, Add Existing File. The resource name will be similar but not exactly the same as the DLL file name.</item>
    /// <item>In a static constructor of your application, call EmbeddedDllClass.ExtractEmbeddedDlls() for each DLL that is needed</item>
    /// <example>
    ///               EmbeddedDllClass.ExtractEmbeddedDlls("libFrontPanel-pinv.dll", Properties.Resources.libFrontPanel_pinv);
    /// </example>
    /// <item>Optional: In a static constructor of your application, call EmbeddedDllClass.LoadDll() to load the DLLs you have extracted. This is not necessary for pinvoke</item>
    /// <example>
    ///               EmbeddedDllClass.LoadDll("myscrewball.dll");
    /// </example>
    /// <item>Continue using standard Pinvoke methods for the desired functions in the DLL</item>
    /// </list>
    /// </summary>
    public class EmbeddedUnmanagedDll
    {
        private static string tempFolder = "";

        /// <summary>
        /// Extract DLLs from resources to temporary folder
        /// </summary>
        /// <param name="dllName">name of DLL file to create (including dll suffix)</param>
        /// <param name="resourceBytes">The resource name (fully qualified)</param>
        public static void ExtractEmbeddedDlls(string embeddedResource, string dllName, bool compressed, out string containedDllName)
        {
            containedDllName = dllName;

            Assembly assem = Assembly.GetExecutingAssembly();
            string[] names = assem.GetManifestResourceNames();
            AssemblyName an = assem.GetName();

            // The temporary folder holds one or more of the temporary DLLs
            // It is made "unique" to avoid different versions of the DLL or architectures.
            // tempFolder = String.Format("{0}.{1}.{2}", an.Name, an.ProcessorArchitecture, an.Version);
            tempFolder = Settings.RelHaxLibrariesFolder;

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            // Add the temporary dirName to the PATH environment variable (at the head!)
            string path = Environment.GetEnvironmentVariable("PATH");
            string[] pathPieces = path.Split(';');
            bool found = false;
            foreach (string pathPiece in pathPieces)
            {
                if (pathPiece == tempFolder)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Environment.SetEnvironmentVariable("PATH", tempFolder + ";" + path);
            }

            // bool fileOk = false;
            string tempFile = "";
            Assembly curAsm = Assembly.GetExecutingAssembly();
            byte[] resourceBytes = null;

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
                {
                    // Either the file is not existed or it is not mark as embedded resource
                    if (stm == null)
                        throw new Exception(embeddedResource + " is not found in Embedded Resources.");

                    // Get byte[] from the file from embedded resource
                    resourceBytes = new byte[(int)stm.Length];
                    stm.Read(resourceBytes, 0, (int)stm.Length);
                }

                // Get the hash value from embedded DLL/assembly
                string fileHash = BitConverter.ToString(sha1.ComputeHash(resourceBytes)).Replace("-", string.Empty);

                // Define the temporary storage location of the DLL/assembly
                tempFile = Path.Combine(tempFolder, dllName);

                // Determines whether the DLL/assembly is existed or not
                if (File.Exists(tempFile))
                {
                    // Get the hash value of the existed file
                    byte[] bb = File.ReadAllBytes(tempFile);
                    string fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);

                    // Compare the existed DLL/assembly with the Embedded DLL/assembly
                    if (fileHash == fileHash2)
                    {
                        // Same file
                        // fileOk = true;
                        return;
                    }
                    // else
                    // {
                    // Not same
                    // fileOk = false;
                    // }
                }
                // else
                // {
                // The DLL/assembly is not existed yet
                // fileOk = false;
                // }
            }

            if (compressed)
                try
                {
                    Utils.GetByteArryFromZip(ref resourceBytes, out dllName);
                    containedDllName = dllName;
                }
                catch (Exception ex)
                {
                    Logging.Manager(string.Format("Exception at UnZip: {0} ({1})", dllName, ex.Message));
                    return;
                }

            // See if the file exists, avoid rewriting it if not necessary
            string dllPath = Path.Combine(tempFolder, dllName);
            bool rewrite = true;
            if (File.Exists(dllPath))
            {
                byte[] existing = File.ReadAllBytes(dllPath);
                if (resourceBytes.SequenceEqual(existing))
                {
                    rewrite = false;
                }
            }
            if (rewrite)
            {
                File.WriteAllBytes(dllPath, resourceBytes);
            }
            return;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// managed wrapper around LoadLibrary
        /// </summary>
        /// <param name="dllName"></param>
        static public void LoadDll(string dllName)
        {
            if (tempFolder == "")
            {
                throw new Exception("Please call ExtractEmbeddedDlls before LoadDll");
            }
            IntPtr h = LoadLibrary(dllName);
            if (h == IntPtr.Zero)
            {
                Exception e = new Win32Exception();
                throw new DllNotFoundException("Unable to load library: " + dllName + " from " + tempFolder, e);
            }
        }
    }
}
