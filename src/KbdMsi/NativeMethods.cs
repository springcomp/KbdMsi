using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Win32
{
	public static class NativeMethods
	{
		public static readonly bool Is64BitProcess = (IntPtr.Size == 8);
		public static readonly bool Is32BitProcess = !Is64BitProcess;
		public static readonly bool Is64BitOperatingSystem = Is64BitProcess || InternalCheckIsWow64();

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWow64Process(
			[In] IntPtr hProcess,
			[Out] out bool wow64Process
		);

		private static bool InternalCheckIsWow64()
		{
			if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
				Environment.OSVersion.Version.Major >= 6)
			{
				using Process p = Process.GetCurrentProcess();
				if (IsWow64Process(p.Handle, out bool retVal))
					return retVal;
				return false;
			}
			else
			{
				return false;
			}
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

		[DllImport("user32.dll", CharSet = CharSet.Ansi)]
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