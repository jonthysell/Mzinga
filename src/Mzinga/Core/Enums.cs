// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Mzinga.Core
{
    public enum PlayerColor
    {
        White = 0,
        Black,
        NumPlayerColors,
    };

    public enum BoardState
    {
        NotStarted = 0,
        InProgress,
        Draw,
        WhiteWins,
        BlackWins,
    };

    [DefaultValue(INVALID)]
    public enum PieceName
    {
        INVALID = -1,
        wQ = 0,
        wS1,
        wS2,
        wB1,
        wB2,
        wG1,
        wG2,
        wG3,
        wA1,
        wA2,
        wA3,
        wM,
        wL,
        wP,
        bQ,
        bS1,
        bS2,
        bB1,
        bB2,
        bG1,
        bG2,
        bG3,
        bA1,
        bA2,
        bA3,
        bM,
        bL,
        bP,
        NumPieceNames
    };

    public enum Direction
    {
        Up = 0,
        UpRight = 1,
        DownRight = 2,
        Down = 3,
        DownLeft = 4,
        UpLeft = 5,
        NumDirections = 6,
#pragma warning disable CA1069 // Enums values should not be duplicated
        Above = 6,
#pragma warning restore CA1069 // Enums values should not be duplicated
    };

    [DefaultValue(INVALID)]
    public enum BugType
    {
        INVALID = -1,
        QueenBee = 0,
        Spider,
        Beetle,
        Grasshopper,
        SoldierAnt,
        Mosquito,
        Ladybug,
        Pillbug,
        NumBugTypes,
    };

    [DefaultValue(INVALID)]
    public enum GameType
    {
        INVALID = -1,
        Base = 0,
        BaseM,
        BaseL,
        BaseP,
        BaseML,
        BaseMP,
        BaseLP,
        BaseMLP,
        NumGameTypes,
    };

    public static class Enums
    {
        public static bool GameInProgress(BoardState boardState)
        {
            return boardState == BoardState.NotStarted || boardState == BoardState.InProgress;
        }

        public static bool GameIsOver(BoardState boardState)
        {
            return boardState == BoardState.WhiteWins || boardState == BoardState.BlackWins || boardState == BoardState.Draw;
        }

        public static PlayerColor GetColor(PieceName value)
        {
            switch (value)
            {
                case PieceName.wQ:
                case PieceName.wS1:
                case PieceName.wS2:
                case PieceName.wB1:
                case PieceName.wB2:
                case PieceName.wG1:
                case PieceName.wG2:
                case PieceName.wG3:
                case PieceName.wA1:
                case PieceName.wA2:
                case PieceName.wA3:
                case PieceName.wM:
                case PieceName.wL:
                case PieceName.wP:
                    return PlayerColor.White;
                case PieceName.bQ:
                case PieceName.bS1:
                case PieceName.bS2:
                case PieceName.bB1:
                case PieceName.bB2:
                case PieceName.bG1:
                case PieceName.bG2:
                case PieceName.bG3:
                case PieceName.bA1:
                case PieceName.bA2:
                case PieceName.bA3:
                case PieceName.bM:
                case PieceName.bL:
                case PieceName.bP:
                    return PlayerColor.Black;
            }

            return PlayerColor.NumPlayerColors;
        }

        public static bool TryGetBugNum(PieceName value, out int bugNum)
        {
            bugNum = GetBugNum(value);
            return bugNum > 0;
        }

        public static int GetBugNum(PieceName value)
        {
            switch (value)
            {
                case PieceName.wS1:
                case PieceName.wB1:
                case PieceName.wG1:
                case PieceName.wA1:
                case PieceName.bS1:
                case PieceName.bB1:
                case PieceName.bG1:
                case PieceName.bA1:
                    return 1;
                case PieceName.wS2:
                case PieceName.wB2:
                case PieceName.wG2:
                case PieceName.wA2:
                case PieceName.bS2:
                case PieceName.bB2:
                case PieceName.bG2:
                case PieceName.bA2:
                    return 2;
                case PieceName.wG3:
                case PieceName.wA3:
                case PieceName.bG3:
                case PieceName.bA3:
                    return 3;
            }

            return 0;
        }

        public static Direction LeftOf(Direction value)
        {
            return (Direction)(((int)value + (int)Direction.NumDirections - 1) % (int)Direction.NumDirections);
        }

        public static Direction RightOf(Direction value)
        {
            return (Direction)(((int)value + 1) % (int)Direction.NumDirections);
        }

        public static BugType GetBugType(PieceName value)
        {
            switch (value)
            {
                case PieceName.wQ:
                case PieceName.bQ:
                    return BugType.QueenBee;
                case PieceName.wS1:
                case PieceName.wS2:
                case PieceName.bS1:
                case PieceName.bS2:
                    return BugType.Spider;
                case PieceName.wB1:
                case PieceName.wB2:
                case PieceName.bB1:
                case PieceName.bB2:
                    return BugType.Beetle;
                case PieceName.wG1:
                case PieceName.wG2:
                case PieceName.wG3:
                case PieceName.bG1:
                case PieceName.bG2:
                case PieceName.bG3:
                    return BugType.Grasshopper;
                case PieceName.wA1:
                case PieceName.wA2:
                case PieceName.wA3:
                case PieceName.bA1:
                case PieceName.bA2:
                case PieceName.bA3:
                    return BugType.SoldierAnt;
                case PieceName.wM:
                case PieceName.bM:
                    return BugType.Mosquito;
                case PieceName.wL:
                case PieceName.bL:
                    return BugType.Ladybug;
                case PieceName.wP:
                case PieceName.bP:
                    return BugType.Pillbug;
                default:
                    return BugType.INVALID;
            }
        }

        public static bool TryParse(string str, out GameType result)
        {
            switch (str)
            {
                case "Base":
                    result = GameType.Base;
                    return true;
                case "Base+M":
                    result = GameType.BaseM;
                    return true;
                case "Base+L":
                    result = GameType.BaseL;
                    return true;
                case "Base+P":
                    result = GameType.BaseP;
                    return true;
                case "Base+ML":
                    result = GameType.BaseML;
                    return true;
                case "Base+MP":
                    result = GameType.BaseMP;
                    return true;
                case "Base+LP":
                    result = GameType.BaseLP;
                    return true;
                case "Base+MLP":
                    result = GameType.BaseMLP;
                    return true;
                default:
                    result = GameType.INVALID;
                    return false;
            }
        }

        public static string GetGameTypeString(GameType value)
        {
            switch (value)
            {
                case GameType.Base:
                    return "Base";
                case GameType.BaseM:
                    return "Base+M";
                case GameType.BaseL:
                    return "Base+L";
                case GameType.BaseP:
                    return "Base+P";
                case GameType.BaseML:
                    return "Base+ML";
                case GameType.BaseMP:
                    return "Base+MP";
                case GameType.BaseLP:
                    return "Base+LP";
                case GameType.BaseMLP:
                    return "Base+MLP";
                default:
                    return "";
            }
        }

        private static readonly uint[] _pieceNameIsEnabledForGameType = new uint[]
        {
            0b1111111111100011111111111000,
            0b1111111111110011111111111100,
            0b1111111111101011111111111010,
            0b1111111111100111111111111001,
            0b1111111111111011111111111110,
            0b1111111111110111111111111101,
            0b1111111111101111111111111011,
            0b1111111111111111111111111111,
        };

        public static bool PieceNameIsEnabledForGameType(PieceName pieceName, GameType gameType)
        {
            if (pieceName == PieceName.INVALID || gameType == GameType.INVALID)
            {
                return false;
            }

            return ((0b1000000000000000000000000000 >> (int)pieceName) & _pieceNameIsEnabledForGameType[(int)gameType]) != 0;
        }

        public static bool BugTypeIsEnabledForGameType(BugType bugType, GameType gameType)
        {
            switch (bugType)
            {
                case BugType.INVALID:
                    return false;
                case BugType.Mosquito:
                    return gameType == GameType.BaseM || gameType == GameType.BaseML || gameType == GameType.BaseMP ||
                           gameType == GameType.BaseMLP;
                case BugType.Ladybug:
                    return gameType == GameType.BaseL || gameType == GameType.BaseML || gameType == GameType.BaseLP ||
                           gameType == GameType.BaseMLP;
                case BugType.Pillbug:
                    return gameType == GameType.BaseP || gameType == GameType.BaseMP || gameType == GameType.BaseLP ||
                           gameType == GameType.BaseMLP;
                default:
                    return true;
            }
        }

        public static GameType EnableBugType(BugType bugType, GameType gameType, bool enabled)
        {
            bool includeM = bugType == BugType.Mosquito ? enabled : BugTypeIsEnabledForGameType(BugType.Mosquito, gameType);
            bool includeL = bugType == BugType.Ladybug ? enabled : BugTypeIsEnabledForGameType(BugType.Ladybug, gameType);
            bool includeP = bugType == BugType.Pillbug ? enabled : BugTypeIsEnabledForGameType(BugType.Pillbug, gameType);

            if (includeM && includeL && includeP)
            {
                return GameType.BaseMLP;
            }
            else if (includeL && includeP)
            {
                return GameType.BaseLP;
            }
            else if (includeM && includeP)
            {
                return GameType.BaseMP;
            }
            else if (includeM && includeL)
            {
                return GameType.BaseML;
            }
            else if (includeP)
            {
                return GameType.BaseP;
            }
            else if (includeL)
            {
                return GameType.BaseL;
            }
            else if (includeM)
            {
                return GameType.BaseM;
            }

            return GameType.Base;
        }

        public static int NumPieceNames(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Base:
                    return (int)PieceName.NumPieceNames - 6;
                case GameType.BaseM:
                case GameType.BaseL:
                case GameType.BaseP:
                    return (int)PieceName.NumPieceNames - 4;
                case GameType.BaseML:
                case GameType.BaseMP:
                case GameType.BaseLP:
                    return (int)PieceName.NumPieceNames - 2;
                case GameType.BaseMLP:
                default:
                    return (int)PieceName.NumPieceNames;
            }
        }

        public static int NumBugTypes(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.Base:
                    return (int)BugType.NumBugTypes - 3;
                case GameType.BaseM:
                case GameType.BaseL:
                case GameType.BaseP:
                    return (int)BugType.NumBugTypes - 2;
                case GameType.BaseML:
                case GameType.BaseMP:
                case GameType.BaseLP:
                    return (int)BugType.NumBugTypes - 1;
                case GameType.BaseMLP:
                default:
                    return (int)BugType.NumBugTypes;
            }
        }
    }
}