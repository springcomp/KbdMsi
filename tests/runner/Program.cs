using System;
using System.IO;

using KbdMsi;

string? operation = null;
string? path = null;
string? lcid = null;
string? productCode = null;

for (var index = 0; index < args.Length; index++)
{
	var arg = args[index];
	switch (arg)
	{
		case "--help":
		case "/?":
			Usage();
			break;
		case "--lcid":
			if (index >= args.Length) ExpectedArgument(arg);
			lcid = args[++index];
			operation = "RegisterKeyboard";
			break;
		case "--path":
			if (index >= args.Length) ExpectedArgument(arg);
			path = args[++index];
			operation = "RegisterKeyboard";
			break;
		case "--product-code":
			if (index >= args.Length) ExpectedArgument(arg);
			productCode = args[++index];
			break;
		default:
			if (index == 0) operation = arg;
			break;
	}
}
if (productCode == null)
	ArgumentCount("Missing required '--product-code' option.");
if (path != null && lcid == null)
	ArgumentCount("Argument '--lcid' is required when using '--path' option.");
if (operation == null)
	ArgumentCount("Missing required operation");

string[] operations = [
	"REGISTERKEYBOARD",
	"ADDKEYBOARDTOLANGBAR",
	"REMOVEKEYBOARDFROMLANGBAR",
	"UNREGISTERKEYBOARD",
];

if (!operations.Contains(operation!.ToUpperInvariant()))
	SyntaxError($"Unknown operation '{operation}' specified");

var code = EnsureProductCode(productCode!);

if (path != null && lcid != null)
	RegisterKeyboard(code, path!, lcid!);

switch (operation!.ToUpperInvariant())
{
	case "ADDKEYBOARDTOLANGBAR":
		AddKeyboardToLangBar(code);
		break;
	case "REMOVEKEYBOARDFROMLANGBAR":
		RemoveKeyboardFromLangBar(code);
		break;
	case "UNREGISTERKEYBOARD":
		UnregisterKeyboard(code);
		break;
}

Environment.Exit(0);

void Usage()
{
	Console.WriteLine("Usage:");
	Console.WriteLine("  runner --help");
	Console.WriteLine("  runner [<OPERATION>] --product-code <MSI-PRODUCTCODE> --path <PATH> --lcid <LCID>");
	Console.WriteLine("  runner <OPERATION> --product-code <MSI-PRODUCTCODE>");
	Console.WriteLine();
	Console.WriteLine("Options:");
	Console.WriteLine("  --help:         Display this help screen.");
	Console.WriteLine("  --lcid:         Locale identifier for the layout.");
	Console.WriteLine("  --path:         path to the keyboard layout file.");
	Console.WriteLine("  --product-code: MSI package product code.");
	Console.WriteLine();
	Console.WriteLine("Operations:");
	Console.WriteLine("  RegisterKeyboard");
	Console.WriteLine("  AddKeyboardToLangBar");
	Console.WriteLine("  RemoveKeyboardFromLangBar");
	Console.WriteLine("  UnregisterKeyboard");
	Console.WriteLine();
	Console.WriteLine("Examples:");
	Console.WriteLine(" runner RegisterKeyboard --product-code \"{55A3FA3E-9897-4E71-91E3-E5BFF615397F}\" --path Layout01.dll --lcid 040c");
	Console.WriteLine(" runner UnregisterKeyboard --product-code \"{55A3FA3E-9897-4E71-91E3-E5BFF615397F}\"");

	Environment.Exit(1);
}
void ArgumentCount(string message)
{
	Console.Error.WriteLine("Wrong argument count.");
	Console.Error.WriteLine(message);
	Environment.Exit(2);
}
void ExpectedArgument(string arg)
{
	Console.Error.WriteLine("Wrong syntax.");
	Console.Error.WriteLine($"Missing required argument for '{arg}' option.");
	Environment.Exit(3);
}
void SyntaxError(string message)
{
	Console.Error.WriteLine("Wrong syntax.");
	Console.Error.WriteLine(message);
	Environment.Exit(4);
}
string EnsureKeyboardLayoutFile(string path)
{
	var fileName = Path.GetFileName(path);
	if (!fileName.EndsWith(".dll"))
		fileName = Path.ChangeExtension(fileName, ".dll");

	var sysFolder = Environment.SystemDirectory;
	var expectedPath = Path.Combine(sysFolder, fileName);

	var actualFolder = Path.GetDirectoryName(path);
	actualFolder = string.IsNullOrWhiteSpace(actualFolder)
		? sysFolder
		: new FileInfo(path).Directory!.FullName
		;
	var actualPath = Path.Combine(actualFolder, fileName);

	if (String.Compare(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase) != 0)
	{
		Console.Error.WriteLine(@"Wrong path.");
		Console.Error.WriteLine($"Invalid '{actualPath}' path specified.");
		Console.Error.WriteLine($"Expected '{expectedPath}' path instead.");
		Environment.Exit(10);
	}

	if (!File.Exists(expectedPath))
	{
		Console.Error.WriteLine(@"Path not found.");
		Console.Error.WriteLine($"Missing required '{expectedPath}' file.");
		Environment.Exit(10);
	}

	return expectedPath;
}
Guid EnsureProductCode(string productCode)
{
	if (Guid.TryParse(productCode, out var code))
		return code;

	Console.Error.WriteLine("Invalid product code.");
	Console.Error.WriteLine($"The value '{productCode}' is not a valid MSI product code.");
	Environment.Exit(11);

	System.Diagnostics.Debug.Assert(false);
	return Guid.Empty;
}
void RegisterKeyboard(Guid code, string path, string lcid)
{
	var layoutPath = EnsureKeyboardLayoutFile(path);
	KeyboardLayoutUtils.RegisterKeyboard(code, layoutPath, lcid);
}
void AddKeyboardToLangBar(Guid productCode) { }
void RemoveKeyboardFromLangBar(Guid productCode) { }
void UnregisterKeyboard(Guid productCode) { }