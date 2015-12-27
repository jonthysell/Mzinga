# Mzinga #

Mzinga is an open-source AI to play the board game Hive. It is architected similar to various chess-playing AIs, consisting of:

* A set of standardized commands similar to the Universal Chess Interface
* An command-line reference engine, which accepts the standardized commands
* A general board UI which can interface with any engine that 

The goal is not to simply implement Hive in code, but to establish a standard way for programmers to create their own AI players. The hope is that this will encourage exploring what it means to create a strong Hive AI.

## Current Status ##

Mzinga.Core implements all of the rules of the base Hive game (no expansion pieces). Mzinga.CoreTest has some basic unit tests. Mzinga.Engine implements enough commands in order to play a game of Hive. There is no GUI.

The AI currently looks at the set of valid moves, and if it sees a move that will cause it to win the game (completely surrounding the enemy Queen) it takes it, otherwise it picks a move randomly.


Hive (C) 2010 Gen42 Games. Mzinga is in no way associated with or endorsed by Gen42 Games. To learn more about Hive, see http://www.hivegame.com.