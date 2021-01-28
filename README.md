# Mzinga #

![CI Build](https://github.com/jonthysell/Mzinga/workflows/CI%20Build/badge.svg)

Mzinga is a collection of open-source software to play the board game [Hive](https://gen42.com/games/hive), with the primary goal of building a community of developers who create Hive-playing AIs.

To that end, Mzinga proposes a [Universal Hive Protocol](https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol) to support interoperability for Hive-playing software.

For more information, please check out the [Mzinga Wiki](https://github.com/jonthysell/Mzinga/wiki).

## Installation ##

Mzinga was written in C# and should run anywhere that supports [.NET Core 3.1](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md). It has been officially tested on:

* Windows 10
* Ubuntu 20.04
* macOS 10.15

### Windows ###

The Windows release provides self-contained x86 binaries and runs on Windows 7 SP1+, 8.1, and 10.

1. Download the latest Windows zip file (Mzinga.Windows.zip) from https://github.com/jonthysell/Mzinga/releases/latest
2. Extract the zip file

### MacOS ###

The MacOS release provides self-contained x64 binaries and runs on OSX >= 10.13.

1. Download the latest MacOS tar.gz file (Mzinga.MacOS.tar.gz) from https://github.com/jonthysell/Mzinga/releases/latest
2. Extract the tar.gz file

### Linux ###

The Linux release provides self-contained x64 binaries and runs on many Linux distributions.

1. Download the latest Linux tar.gz file (Mzinga.Linux.tar.gz) from https://github.com/jonthysell/Mzinga/releases/latest
2. Extract the tar.gz file

## Projects ##

Mzinga is composed of two main projects, the Engine and the Viewer.

### Mzinga.Engine ###

Mzinga.Engine.exe is Mzinga's engine, a command-line application through which you can play a game of Hive. It accepts input commands and outputs results according to the specifications of the Universal Hive Protocol.

### Mzinga.Viewer (Windows-only) ###

Mzinga.Viewer.exe is Mzinga's viewer, a graphical application which can drive Mzinga.Engine (or any engine that implements the specifications of the Universal Hive Protocol).

Mzinga.Viewer is not meant to be graphically impressive or compete with commercial versions of Hive, but rather be a ready-made UI for developers who'd rather focus their time on building a compatible engine and AI.

## Other Projects ##

### Mzinga.Perft ###

Mzinga.Perft.exe is a command-line utility for measuring Mzinga's performance by running [Perft](https://github.com/jonthysell/Mzinga/wiki/Perft).

### Mzinga.Test ###

Mzinga.Test.dll contains unit tests for Mzinga.

### Mzinga.Trainer ###

Mzinga.Trainer.exe is a command-line utility with the goal to improve Mzinga's AI. Through it you can generate randomized AI profiles and execute AI vs. AI battles.

## Copyright ##

Hive Copyright (c) 2016 Gen42 Games. Mzinga is in no way associated with or endorsed by Gen42 Games.

Mzinga Copyright (c) 2015-2021 Jon Thysell.

MVVM Light Toolkit Copyright (c) 2009-2018 Laurent Bugnion.

Extended WPF Toolkit Copyright (c) 2010-2019 Xceed Software Inc.
