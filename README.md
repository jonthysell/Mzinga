![Mzinga Banner](./.github/banner.png)

# Mzinga #

[![CI Build](https://github.com/jonthysell/Mzinga/actions/workflows/ci.yml/badge.svg)](https://github.com/jonthysell/Mzinga/actions/workflows/ci.yml)

Mzinga is a collection of open-source software to play the board game [Hive](https://gen42.com/games/hive), with the primary goal of building a community of developers who create Hive-playing AIs.

To that end, Mzinga proposes a [Universal Hive Protocol](https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol) to support interoperability for Hive-playing software.

For more information on Mzinga and its projects, please check out the [Mzinga Wiki](https://github.com/jonthysell/Mzinga/wiki).

## Installation ##

Mzinga was written in C# and should run anywhere that supports [.NET 6.0](https://github.com/dotnet/core/blob/main/release-notes/6.0/supported-os.md). It has been officially tested on:

* Windows 10 and 11
* Ubuntu 20.04
* macOS 10.15

### Windows ###

#### Standard ####

The standard Windows release provides self-contained x86/x64/arm64 binaries which run on Windows 7 SP1+, 8.1, 10, and 11.

1. Download the latest Windows zip file (Mzinga.WinX86.zip, Mzinga.WinX64.zip, *or* Mzinga.WinArm64.zip) from https://github.com/jonthysell/Mzinga/releases/latest
2. Extract the zip file

**Note:** If you're unsure which version to download, try Mzinga.WinX64.zip first. Most modern PCs are x64.

#### Setup MSI ####

A standalone version of *MzingaViewer* is available via an installer which can run on Windows 7 SP1+, 8.1, 10, and 11.

1. Download the latest Windows setup file (MzingaViewer.WinX86.Setup.msi, MzingaViewer.WinX64.Setup.msi *or* MzingaViewer.WinArm64.Setup.msi) from https://github.com/jonthysell/Mzinga/releases/latest
2. Open the installer

**Note:** If you're unsure which version to download, try MzingaViewer.WinX64.Setup.msi first. Most modern PCs are x64.

**Note:** This version does not contain the *MzingaEngine*, *MzingaPerft*, or *MzingaTrainer* binaries.

#### Microsoft Store ####

A standalone version of *MzingaViewer* is available via the Microsoft Store for Windows 10 and 11: https://www.microsoft.com/en-us/p/mzingaviewer/9pm7p224hpgq

**Note:** This version does not contain the *MzingaEngine*, *MzingaPerft*, or *MzingaTrainer* binaries.

### MacOS ###

The MacOS release provides self-contained x64/arm64 binaries which run on OSX >= 10.15.

1. Download the latest MacOS tar.gz file (Mzinga.MacOSX64.tar.gz *or* Mzinga.MacOSArm64.tar.gz) from https://github.com/jonthysell/Mzinga/releases/latest
2. Extract the tar.gz file

**Note:** If you're unsure which version to download, try Mzinga.MacOSX64.tar.gz first. Most modern PCs are x64.

**Note:** If MacOS Gatekeeper prevents you from running Mzinga, you may need to run `xattr -cr` on the downloaded files.

### Linux ###

The Linux release provides self-contained x64/arm64 binaries which run on many Linux distributions.

1. Download the latest Linux tar.gz file (Mzinga.LinuxX64.tar.gz *or* Mzinga.LinuxArm64.zip) from https://github.com/jonthysell/Mzinga/releases/latest
2. Extract the tar.gz file

**Note:** If you're unsure which version to download, try Mzinga.LinuxX64.tar.gz first. Most modern PCs are x64.

### Unpacked ###

The Unpacked release provides loose, framework-dependent .NET 6 binaries.

1. Install the latest [.NET Runtime](https://dotnet.microsoft.com/download/dotnet/6.0)
2. Download the latest Unpacked zip file (Mzinga.Unpacked.zip) from https://github.com/jonthysell/Mzinga/releases/latest
3. Extract the zip file

## Copyright ##

Hive Copyright (c) 2016 Gen42 Games. Mzinga is in no way associated with or endorsed by Gen42 Games.

Mzinga Copyright (c) 2015-2024 Jon Thysell.

Avalonia Copyright (c) .NET Foundation and Contributors.

Markdown.Avalonia Copyright (c) 2010 Bevan Arps, 2020 Whistyun.

Mono.Unix Copyright (c) 2021 Mono Project.

MVVM Toolkit Copyright (c) .NET Foundation and Contributors.
