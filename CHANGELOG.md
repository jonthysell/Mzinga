# Mzinga ChangeLog #

## next ##

* Viewer: Remove deprecated "Mzinga" notation type

## v0.10.2 ##

* Core: FastCore refactor to simplify code and to improve build and runtime performance

## v0.10.1 ##

* Core: Fixed missing valid Spider moves
* Engine: Perft calculations are no longer parallelized
* Perft: Calculations can be parallelized with "-mt" flag
* Internal code cleanup

## v0.10 ##

* Ported projects to .NET 5
* Viewer: Rebuilt on Avalonia, now runs on MacOS and Linux
* Viewer: Added Light / Dark theme option
* Viewer: Properly send SIGINT signal to cancel engine processing Linux and MacOS
* Engine: Properly handle incoming SIGINT signals to cancel engine processing on Linux and MacOS

## v0.9.21000.0 ##

* Ported projects to .NET Core 3.1
* First Linux, MacOS builds for Engine, Trainer, Perft
* No MSI installer, "portable" build only
