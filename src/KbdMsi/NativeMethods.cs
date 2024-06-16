using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Win32
{
	public static class NativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int LoadString(IntPtr hInstance, int ID, StringBuilder lpBuffer, int nBufferMax);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeLibrary(IntPtr hModule);

		public static string ExtractStringFromDLL(string file, int number)
		{
			IntPtr lib = LoadLibrary(file);
			try
			{
				StringBuilder result = new StringBuilder(2048);
				LoadString(lib, number, result, result.Capacity);
				return result.ToString();
			}
			finally
			{
				FreeLibrary(lib);
			}
		}
	}
}