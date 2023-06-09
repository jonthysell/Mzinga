# Mzinga Changelog #

## next ##

* New icon
* Viewer: Minor rendering improvements
* Viewer: Switched to CompiledBindings for improved performance
* Viewer: Updated New Game Window
* Viewer: Updated Avalonia to 11.0.0-rc1.1
* Viewer: Updated Markdown.Avalonia to Markdown.Avalonia.Tight 11.0.0-d1

## v0.13.6 ##

* Core: Moved GameRecording code into Core
* Test: Added tests for SGF/PGN loading/saving
* Viewer: Fixed issue with resetting default sidebar viewer option
* Viewer: Improved SGF parsing to handle SGF start/end of game markers
* Viewer: Improved SGF parsing to handle draw offers
* Viewer: Improved SGF parsing to handle movedone command

## v0.13.5 ##

* Viewer: Fixed issue with loading newer BoardSpace SGF files which record pass moves differently

## v0.13.4 ##

* Viewer: Engine Console now includes engine's std:err output

## v0.13.3 ##

* Core: Fixed issue with parsing moves next to pieces not in play
* Viewer: Fixed "crash" when exiting while Internal Engine is still running
* Viewer: Fixed issues when loading newer BoardSpace SGF files
* Viewer: Added detection of PGN and SGF files without filename extensions
* Viewer: Fixed issue with overwriting existing PGN files
* Viewer: Fixed issue with move commentary not updating
* Viewer: Updated Avalonia to 0.10.19
* Viewer: Updated Markdown.Avalonia to Markdown.Avalonia.Tight 0.10.13

## v0.13.2 ##

* Viewer: Updated Avalonia to 0.10.18
* Viewer: Updated MVVM Toolkit to 8.0.0

## v0.13.1 ##

* Core: Update version string to always display Major.Minor.Build
* Viewer: Updated WinStore TargetPlatformVersion to 10.0.19041.0

## v0.13.0 ##

* Core: Allow for no (zero-sized) transposition table
* Core: Cache sorted move lists at each position
* Engine: Changed defaults and acceptable ranges for options
* Engine: Exposed QuiescentSearchMaxDepth option
* Engine: Exposed UseNullAspirationWindow option
* Engine: Fixed issue with never using end metric weights
* Engine: Fixed issue with MaxHelperThreads == Auto on machines with only 1 processor
* Engine: Migrated from deprecated Mono.Posix to new Mono.Unix
* Test: Improve debugging of GameAI tests
* Test: Improve GameAI tests with configurable options matrix
* Test: Changed GameAI TreeStrap test to verify best move is maintained after training
* Viewer: Fix issue when using newgame via the Engine Console not updating the GameType metadata
* Viewer: Fix crash when playing AI vs AI games with too short search times
* Viewer: Improve BoardHistory UI and Review Mode performance
* Viewer: Sort options in Engine Options dialog
* Viewer: Engine console now scrolls to keep up with the latest output
* Viewer: Updated Avalonia to 0.10.16
* Viewer: Migrated from deprecated MVVM Light Toolkit to new MVVM Toolkit
* Viewer: Migrated from deprecated Mono.Posix to new Mono.Unix

## v0.12.9 ##

* Core: Fixed conflict with Pillbugs and Mosquitos adding duplicate moves

## v0.12.8 ##

* Core: Optimized CalculateValidPlacements
* Core: Optimized Enums.PieceNameIsEnabledForGameType and Board.PlacingPieceInOrder for better performance
* Core: Optimized FastSet by reversing search order
* Core: Optimized Move equality comparison
* Core: Optimized Position.GetHashCode
* Core: Expanded use of MoveSet.FastAdd for certain bugs' moves
* Test: Added more move tests based on final matches of the Online Hive World Championships 

## v0.12.7 ##

* Core: Set initial PositionSet capacity to improve performance
* Core: Fixed broken BoardMetrics calculations
* Core: Empty board should always be scored as a zero
* Core: Updated DefaultEngineConfig.xml with latest mergetop results
* Test: Added TreeStrap tests
* Trainer: AutoTrain now supports using MzingaAutoTrainConfig.xml

## v0.12.6 ##

* Viewer: Set focus to input when opening Engine Console
* Viewer: Show engine id at top of Engine menu
* Viewer: Fix relative links when showing update release notes

## v0.12.5 ##

* Viewer: Show release details when prompting to update
* Viewer: Fix invalid thread error when new update detected at app start

## v0.12.4 ##

* Core: Fixed bug with producing (and accepting) invalid UHP GameStrings
* Core: Move generation performance improvement for Spider
* Core: Move generation performance improvement for SoldierAnt
* Perft: Converted project to nullable
* Test: Move tests based on real games
* Viewer: Added error message when opening a saved game of a type the engine cannot play

## v0.12.3 ##

* Core: Fixed spider move generation
* Engine: Converted project to nullable
* Engine: Fixed licenses output
* Engine: Fixed help output

## v0.12.2 ##

* Viewer: Fixed bug where center/zoom buttons don't appear on new game
* Viewer: Added dependency on Markdown.Avalonia
* Viewer: Revamped about window with embedded license and changelog contents

## v0.12.1 ##

* Engine: Fixed issue when program doesn't exit if input pipe closes
* Viewer: Updated Avalonia to 0.10.14

## v0.12 ##

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
* Viewer: Re-arranged center, lift, zoom buttons

## v0.11.4 ##

* Engine: Fixed bug where pondering starts after a game is over, causing an error
* Viewer: Fixed bug where game over message is displayed more than once
* Viewer: Update Avalonia to 0.10.7

## v0.11.3 ##

* Core: Fixed crash when trying to stack all beetles and mosquitoes
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

## v0.11 ##

* Engine/Viewer: Fixed bug where loading default config looked in working directory, not app entry-point
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
