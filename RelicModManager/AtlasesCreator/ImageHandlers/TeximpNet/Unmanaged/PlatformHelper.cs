using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RelhaxModpack.AtlasesCreator.ImageHandlers.TeximpNet.Unmanaged
{
    internal enum OSPlatform
    {
        Windows = 0,
        Linux = 1,
        OSX = 2
    }

    internal static class RuntimeInformation
    {
        private static OSPlatform s_platform;

        static RuntimeInformation()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    if (Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users") && Directory.Exists("/Volumes"))
                        s_platform = OSPlatform.OSX;
                    else
                        s_platform = OSPlatform.Linux;
                    break;
                case PlatformID.MacOSX:
                    s_platform = OSPlatform.OSX;
                    break;
                default:
                    s_platform = OSPlatform.Windows;
                    break;
            }
        }

        public static bool IsOSPlatform(OSPlatform osPlat)
        {
            return s_platform == osPlat;
        }
    }

    //Helper class for making it easier to access certain reflection methods on types between .Net framework and .Net standard (pre-netstandard 2.0)
    internal class PlatformHelper
    {
        public static String GetAppBaseDirectory()
        {
            return RelhaxModpack.Settings.RelHaxLibrariesFolder;
            // return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static bool IsAssignable(Type baseType, Type subType)
        {
            if (baseType == null || subType == null)
                return false;

            return baseType.IsAssignableFrom(subType);
        }

        public static Type[] GetNestedTypes(Type type)
        {
            if (type == null)
                return new Type[0];

            return type.GetNestedTypes();
        }

        public static Object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            if (type == null || attributeType == null)
                return new Object[0];

            return type.GetCustomAttributes(attributeType, inherit);
        }

        //These methods are marked obsolete in earlier netstandard versions, but are not marked as such in 2.0
        public static Delegate GetDelegateForFunctionPointer(IntPtr procAddress, Type delegateType)
        {
            if (procAddress == IntPtr.Zero || delegateType == null)
                return null;

            return Marshal.GetDelegateForFunctionPointer(procAddress, delegateType);
        }

        public static IntPtr GetFunctionPointerForDelegate(Delegate func)
        {
            if (func == null)
                return IntPtr.Zero;

            return Marshal.GetFunctionPointerForDelegate(func);
        }
    }
}
