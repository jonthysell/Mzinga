// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    public class GameSettings
    {
        public PlayerType WhitePlayerType { get; set; } = PlayerType.Human;

        public PlayerType BlackPlayerType { get; set; } = PlayerType.EngineAI;

        public ExpansionPieces ExpansionPieces
        {
            get
            {
                return Metadata.GameType;
            }
            set
            {
                Metadata.SetTag("GameType", EnumUtils.GetExpansionPiecesString(value));
            }
        }

        public BestMoveType BestMoveType
        {
            get
            {
                return _bestMoveType;
            }
            set
            {
                if (_bestMoveType != value)
                {
                    _bestMoveType = value;
                    if (value == BestMoveType.MaxDepth)
                    {
                        BestMoveMaxTime = null;
                        BestMoveMaxDepth = DefaultMaxDepth;
                    }
                    else
                    {
                        BestMoveMaxDepth = null;
                        BestMoveMaxTime = DefaultMaxTime;
                    }
                }
            }
        }
        private BestMoveType _bestMoveType = BestMoveType.MaxTime;

        public int? BestMoveMaxDepth
        {
            get
            {
                return _bestMoveMaxDepth;
            }
            set
            {
                if (value.HasValue && value.Value < 0)
                {
                    value = null;
                }
                _bestMoveMaxDepth = value;
            }
        }
        private int? _bestMoveMaxDepth = null;

        public TimeSpan? BestMoveMaxTime
        {
            get
            {
                return _bestMoveMaxTime;
            }
            set
            {
                if (value.HasValue && value.Value < TimeSpan.Zero)
                {
                    value = null;
                }
                _bestMoveMaxTime = value;
            }
        }
        private TimeSpan? _bestMoveMaxTime = DefaultMaxTime;

        public GameRecording GameRecording { get; private set; }

        public GameBoard CurrentGameBoard
        {
            get
            {
                return _currentGameBoard;
            }
            set
            {
                _currentGameBoard = value ?? throw new ArgumentNullException(nameof(value));

                if (GameMode == GameMode.Play)
                {
                    GameRecording = new GameRecording(CurrentGameBoard, GameRecordingSource.Game, GameRecording.Metadata);
                    Metadata.SetTag("Result", CurrentGameBoard.BoardState.ToString());
                }
            }
        }
        private GameBoard _currentGameBoard;

        public GameMetadata Metadata
        {
            get
            {
                return GameRecording.Metadata;
            }
        }

        public GameMode GameMode { get; set; } = GameMode.Play;

        public GameSettings()
        {
            GameRecording = new GameRecording(new GameBoard(), GameRecordingSource.Game);
            _currentGameBoard = GameRecording.GameBoard.Clone();
        }

        public GameSettings(GameRecording gameRecording)
        {
            GameRecording = gameRecording ?? throw new ArgumentNullException(nameof(gameRecording));
            _currentGameBoard = GameRecording.GameBoard.Clone();
        }

        public GameSettings(GameBoard gameBoard, GameMetadata metadata = null)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException(nameof(gameBoard));
            }

            GameRecording = new GameRecording(gameBoard, GameRecordingSource.Game, metadata);
            _currentGameBoard = GameRecording.GameBoard.Clone();
        }

        public GameSettings Clone()
        {
            GameSettings clone = new GameSettings(CurrentGameBoard, Metadata)
            {
                WhitePlayerType = WhitePlayerType,
                BlackPlayerType = BlackPlayerType,

                BestMoveType = BestMoveType,

                BestMoveMaxDepth = BestMoveMaxDepth,
                BestMoveMaxTime = BestMoveMaxTime,

                GameMode = GameMode,
            };

            return clone;
        }

        public static GameSettings CreateNewFromExisting(GameSettings source)
        {
            GameSettings clone = new GameSettings(new GameBoard(source.ExpansionPieces))
            {
                WhitePlayerType = source.WhitePlayerType,
                BlackPlayerType = source.BlackPlayerType,

                BestMoveType = source.BestMoveType,

                BestMoveMaxDepth = source.BestMoveMaxDepth,
                BestMoveMaxTime = source.BestMoveMaxTime,

                GameMode = source.GameMode,
            };

            return clone;
        }

        private static readonly int DefaultMaxDepth= 2;
        private static readonly TimeSpan DefaultMaxTime = TimeSpan.FromSeconds(5.0);
    }

    public enum PlayerType
    {
        Human = 0,
        EngineAI
    }

    public enum BestMoveType
    {
        MaxDepth = 0,
        MaxTime
    }

    public enum GameMode
    {
        Play,
        Review
    }
}
