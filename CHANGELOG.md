# Mzinga ChangeLog #

## vNext ##

* Ported projects to .NET 5
* Viewer: Rebuilt on Avalonia, now runs on MacOS and Linux
* Viewer: Added Light / Dark theme option
* Viewer: Properly send SIGINT signal to cancel engine processing Linux and MacOS
* Engine: Properly handle incoming SIGINT signals to cancel engine processing on Linux and MacOS

## v0.9.21000.0 ##

* Ported projects to .NET Core 3.1
* First Linux, MacOS builds for Engine, Trainer, Perft
* No MSI installer, "portable" build only
