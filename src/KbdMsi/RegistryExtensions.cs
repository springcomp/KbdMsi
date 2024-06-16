using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using Microsoft.Win32;

namespace KbdMsi
{
	public static class RegistryExtensions
	{
		const string REG_KBD_ROOT = "HKLM://SYSTEM/ControlSet001/Control/Keyboard Layouts";

		const NumberStyles P_HEX = NumberStyles.HexNumber;
		static readonly IFormatProvider P_LANG = CultureInfo.InvariantCulture;

		public static IEnumerable<(string, uint)> EnumKeyboardLayouts(this RegistryKey key)
		{
            static uint? IsKeyboardLayout(string name)
				=> uint.TryParse(name, P_HEX, P_LANG, out var id)
					? id
					: (uint?) null
					;

			return key.EnumSubkeyNames(IsKeyboardLayout);

		}
		public static IEnumerable<(string SukeyName, T Selected)> EnumSubkeyNames<T>(this RegistryKey key, Func<string, T?>? selector = null) where T : struct
		{
			foreach (var name in key.GetSubKeyNames())
			{
				var selected = selector?.Invoke(name);
				if (selected != null)
					yield return (name, (T)(object)selected);
			}
		}
	}
}