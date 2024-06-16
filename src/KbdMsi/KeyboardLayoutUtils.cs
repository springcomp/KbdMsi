using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace KbdMsi
{
	public static class KeyboardLayoutUtils
	{
		const string REG_KBD_ROOT = "HKLM://SYSTEM/ControlSet001/Control/Keyboard Layouts";

		const NumberStyles P_HEX = NumberStyles.HexNumber;
		static readonly IFormatProvider P_LANG = CultureInfo.InvariantCulture;

		public static string RegisterKeyboard(Guid productCode, string path, string lcid)
		{
			if (!ushort.TryParse(lcid, P_HEX, P_LANG, out var id))
				throw new ApplicationException($"Invalid locale identifier '{lcid}' specified");

			return RegisterKeyboard(productCode, path, id);
		}

		public static string RegisterKeyboard(Guid productCode, string path, ushort lcid)
		{
			// http://www.kbdedit.com/manual/admin_deploy.html#KLID

			var (klid, lid) = AssignKLID(lcid);

			var layoutFile = Path.GetFileName(path);
			var layoutId = lid.ToString("x4");
			var layoutProductCode = productCode.ToString("B").ToUpperInvariant();

			// The custom language display name and custom language name
			// both come from the string table resources embedded
			// in the layout file, record number 1100.
			//
			// The custom language display name is extracted from
			// the string table resource with the current language identifier.
			// (i.e, the language defined in Windows) using registry string redirection.
			//
			// Whereas the custom language name is extracted from
			// the string table resource and specified explicitly in the registry.
			// For now, use Windows’ native language name for the LCID if string is not found.

			var languageDisplayName = $"@%SystemRoot%\\system32\\${layoutFile},-1100";
			var languageName = Win32.NativeMethods.ExtractStringFromDLL(path, 1100);
			if (languageName == "")
				languageName = GetLanguageName(lcid);

			// The layout display name and layout text
			// both come from the string table resources embedded
			// in the layout file, record number 1000.
			//
			// The layout display name is extracted from
			// the string table resource with the current language identifier.
			// (i.e, the language defined in Windows)
			//
			// While layout display name supports registry string redirection,
			// the layout text property does not. So we need to extract the
			// resource ourselves here

			var layoutDisplayName = $"@%SystemRoot%\\system32\\${layoutFile},-1000";
			var layoutText = Win32.NativeMethods.ExtractStringFromDLL(path, 1000);

			// add the keyboard layout details to the registry

			Console.WriteLine($"{klid:x}");

			using var key = OpenRegistry(REG_KBD_ROOT, writeable: true);
			using var subKey = key.CreateSubKey($"{klid:x8}");

			subKey.SetValue("Custom Language Display Name", languageDisplayName);
			if (languageName != null)
				subKey.SetValue("Custom Language Name", languageName);
			subKey.SetValue("Layout Display Name", layoutDisplayName);
			subKey.SetValue("Layout File", layoutFile);
			subKey.SetValue("Layout Id", layoutId);
			subKey.SetValue("Layout Product Code", layoutProductCode);
			subKey.SetValue("Layout Text", layoutText);

			return $"{klid:x8}";
		}
		public static void UnregisterKeyboard(Guid productCode)
		{
			var expectedLayoutProductCode = productCode.ToString("B").ToUpperInvariant();

			using var key = OpenRegistry(REG_KBD_ROOT, writeable: true);
			foreach (var (name, id) in key.EnumKeyboardLayouts())
			{
                using var subKey = key.OpenSubKey(name);

				// remove all keyboard layouts with that product code
				// from the registry

                var keyValue = subKey.GetValue("Layout Product Code");
                if (keyValue is string actualLayoutProductCode && expectedLayoutProductCode == actualLayoutProductCode)
                {
					Console.WriteLine($"{id:x8}");
					key.DeleteSubKey(name);
                }
            }
		}

		public static string? GetLanguageName(ushort lcid)
		{
			var culture = CultureInfo
				.GetCultures(CultureTypes.AllCultures)
				.FirstOrDefault(c => c.LCID == lcid)
				;

			var languageName = culture?.NativeName;
			if (languageName == null)
				return null;

			var firstLetter = char.ToUpper(languageName[0], culture);
			var remainder = languageName.Length > 1
				? languageName.Substring(1)
				: ""
				;

			return $"{firstLetter}{remainder}";
		}

		/// <summary>
		/// Iterates over the registry to find a free entry for a KLID.
		/// This functions assigns KLID with:
		/// - High-order bits starting from 0xa000 (40960)
		/// - Low-order bits set to the input Locale Code Identifier (LCID)
		/// </summary>
		/// <param name="lcid">locale code identifier</param>
		/// <returns>new keyboard layout identifier</returns>
		public static (int KeyboardLayoutIdentifier, ushort LayoutId) AssignKLID(ushort lcid)
		{
			// MSKLC assigns new KLIDs starting from this number

			ushort newhi = 0xa000;

			// Layout IDs seem to be assigned sequentially
			// However, new official layouts are constantly created
			// Let’s try and use a higher number to reduce conflicts

			ushort newid = 0x0100;

			using var key = OpenRegistry(REG_KBD_ROOT);
			foreach (var (name, id) in key.EnumKeyboardLayouts())
			{
				// record max encountered value for
				// the registry value named "Layout Id"

				using (var subKey = key.OpenSubKey(name))
				{
					var layoutId = subKey.GetValue("Layout Id");
					if (layoutId is string _lid && ushort.TryParse(_lid, out var existingid))
						newid = Math.Max(newid, existingid);
				}

				// record max encountered value for high-order bits

				if ((ushort)(id & 0x0000FFFF) == lcid)
					newhi = Math.Max(newhi, (ushort)(id >> 16));
			}

			var klid = (newhi + 1) << 16 | lcid;
			var lid = (ushort)(newid + 1);

			return (klid, lid);
		}

		private static RegistryKey OpenRegistry(string path, bool writeable = false)
		{
			var root = path.Substring(0, 4);
			var relative = path.Substring(6);

			var key = root switch
			{
				"HKLM" => Registry.LocalMachine,
				_ => throw new NotSupportedException(),
			};
			foreach (var subkey in relative.Split('/'))
			{
				using var k = key;
				key = key.OpenSubKey(subkey, writeable);
			}

			return key;
		}
	}
}