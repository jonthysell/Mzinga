# Mzinga ChangeLog #

## next ##

* Engine: No longer search for default config file
* Viewer: Fix MacOS bundle for MzingaViewer
* Added Mzinga.Viewer.Package project

## v0.10.8 ##

* New icon
* Core: Fixed casing of Move.PassString
* Viewer: Allow parsing game strings from Engine output even if Mzinga thinks the moves are invalid
* Perft: Allow passing in starting GameString

## v0.10.7 ##

* Core: Simplified QueenBee valid moves for better performance
* Core: Fixed wasteful memory usage by GameAI caches
* Core: MoveSet reimplemented with FastAdd to improve performance
* Core: Updated DefaultEngineConfig.xml with latest mergetop results
* Perft: Fixed bug with parsing CLI arguments
* Viewer: Fixed bug with missing Play/Review mode menu
* Test: Added more Perft tests to catch move generator regressions

## v0.10.6 ##

* Core: Fix issues with LastPieceMoved which cause the AI to try to play invalid moves

## v0.10.5 ##

* Viewer: Fixed issues loading SGF files

## v0.10.4 ##

* Core: Fixed a bug with generating the correct move notation
* Engine: Fixed a bug that didn't output every valid move
* Trainer: Fixed bug with exportai command and new version numbers
* Trainer: Added GameString output to exceptions during battle

## v0.10.3 ##

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
