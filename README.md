This repository contains the source code for a set of [Windows Installer Xml (WiX)](https://wixtoolset.org/) toolset [Custom Actions](https://wixtoolset.org/docs/v3/wixdev/extensions/authoring_custom_actions/) written in C#.

The custom actions help design software installation packages (MSI) that can:
- Register one of more keyboard layouts DLL file
- Add a keyboard layout to the Windows Language Bar
- Remove a keyboard layout from the Windows Language Bar
- Unregister a keyboard layout DLL file

## Overview

`Kbdmsi.dll` is a drop-in compatible replacement for [the original DLL with the same name](https://github.com/springcomp/optimized-azerty-win) that ships with
the [Microsoft Keyboard Layout Creator utility (MSKLC)](https://www.microsoft.com/en-us/download/details.aspx?id=102134)

Windows Installer MSI packages produced by MSKC are designed to install and manage a single keyboard layout DLL file.

In order to support MSI packages that host and manage multiple keyboard layout DLL files, the custom actions defined
in `Kbdmsi.dll` now support additional input parameters to identify which keyboard layout to operate on.
This will help design MSI packages that host multiple keyboard layouts, while still being able to, _e.g_, unregister
_only some_ of the keyboard layouts managed by the installation package.
