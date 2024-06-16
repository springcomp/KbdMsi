using System;

namespace KbdMsi {

	public static class KeyboardLayoutUtils
	{
		public static void RegisterKeyboard(string path, string lcid)
		{
			Console.WriteLine("Registering keyboard layout");
			Console.WriteLine($"Path: {path}");
			Console.WriteLine($"LCID: {lcid}");
		}
	}
}