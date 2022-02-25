# Mzinga ChangeLog #

## next ##

* Updated all projects to .NET 6
* Viewer: Updated Avalonia to 0.10.12
* Viewer: Removed dependency on deprecated WebRequest APIs

## v0.11.6 ##

* Restored the MSI install option with the new Mzinga.Viewer.Setup project

## v0.11.5 ##

* Viewer: Added sound support for Windows, MacOS, and Linux
* Viewer: Added options for free panning and zoom of game board
* Viewer: Bottom buttons no longer render directly on top of board pieces
* Viewer: Call out when an error came from the engine
* Viewer: Fixed issue with AI playing too fast causing board state to become corrupted
* Viewer: Fixed issue with trying to play moves in review mode
* Viewer: Fixed issue with trying to play moves that can't even be parsed into move strings
* Viewer: Re-arranged center, lift, zoom buttons"

## v0.11.4 ##

* Engine: Fixed bug where pondering starts after a game is over, causing an error
* Viewer: Fixed bug where game over message is displayed more than once
* Viewer: Update Avalonia to 0.10.7

## v0.11.3 ##

* Core: Fixed crash when trying to stack all beetles and mosquitos
* Core: Fixed puzzle validation
* Core: Fixed race condition where FixedCache.TryLookup accesses an entry being removed
* Core.AI: Fixed OOM crash when getting Principal Variation where the game never ends
* Test: Added more puzzle tests
* Trainer: Added better exception logging during battles
* Viewer: Fixed Store app icons

## v0.11.2 ##

* Core.AI: Add functionality to get Principal Variation
* Core.AI: Improve helper thread performance
* Core.AI: Reduced memory allocations
* Perft: Improve multi-threaded performance
* Perft: Reduced memory allocations
* Test: Added Move parsing tests and fixed bugs
* Trainer: Puzzle candidates are now validated as actual puzzles
* Viewer: Fixed bug where highlighting last move highlights origin when playing new piece

## v0.11.1 ##

* Viewer: Fixed bug with creating folder to save config

## v0.11.0 ##

* Engine/Viewer: Fixed bug where loading default config looked in working directory, not app entrypoint
* Viewer: Enable GPU rendering to improve performance
* Viewer: Fix MacOS bundle for MzingaViewer
* Viewer: Fixed issues with launching CLI engines
* Viewer: Update Avalonia to 0.10.6
* Added Mzinga.Viewer.Package project
* Reduced binary sizes with TrimMode=link

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
