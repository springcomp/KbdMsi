This repository contains the source code for a set of [Windows Installer Xml (WiX)](https://wixtoolset.org/) toolset [Custom Actions](https://wixtoolset.org/docs/v3/wixdev/extensions/authoring_custom_actions/) written in C#.

The custom actions help design software installation packages (MSI) that can:
- Register one of more keyboard layouts DLL file
- Add a keyboard layout to the Windows Language Bar
- Remove a keyboard layout from the Windows Language Bar
- Unregister a keyboard layout DLL file

## Overview

`Kbdmsi.dll` is a drop-in compatible replacement for [the original DLL with the same name](https://github.com/springcomp/optimized-azerty-win) that ships with
the [Microsoft Keyboard Layout Creator utility (MSKLC)](https://www.microsoft.com/en-us/download/details.aspx?id=102134).

While `Kbdmsi.dll` is written in C#, the real WiX toolset custom action DLL is `Kbdmsi.CA.dll`.
That DLL is a native code, WiX toolset-compatible entry-point. It is a self-extractable archive
that embeds and delegates its processing to the `Kbdmsi.dll` implementation.

Although being compatible with MSKLC is a design goal, it is important to recognize MSKLC limitations and opportunities for improvement.

### Support for multi-layout MSI packages

Windows Installer MSI packages produced by MSKLC are designed to install and manage a single keyboard layout DLL file.

In order to support MSI packages that host and manage multiple keyboard layout DLL files, the custom actions defined
in `Kbdmsi.dll` now support additional input parameters to identify which keyboard layout to operate on.
This will help design MSI packages that host multiple keyboard layouts, while still being able to, _e.g_, unregister
_only some_ of the keyboard layouts managed by the installation package.

### Support for Windows on ARM64

Windows Installer MSI packages produced by MSKLC can only install keyboard layouts designed to run on `x86` and `x64` architectures.

In order to support MSI packages that can install keyboard layouts designed for a wider spectrum of software architectures,
including the ARM64-based Windows+ Copilot PCs, `Kbdmsi.dll` is written in C# to make it easy to support the same architectures
that are supported by dotnet.

The `Kbdmsi.dll` [releases](https://github.com/springcomp/KbdMsi/releases) will include an `arm64` target.
