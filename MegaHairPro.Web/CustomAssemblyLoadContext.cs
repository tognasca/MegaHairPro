using System;
using System.Runtime.Loader;

namespace MegaHairPro.Web
{
    public class CustomAssemblyLoadContext
     : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(
            string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }

        protected override IntPtr LoadUnmanagedDll(
            string unmanagedDllName)
        {
            return IntPtr.Zero;
        }
    }

}
