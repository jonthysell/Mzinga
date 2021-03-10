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

        public GameType GameType
        {
            get
            {
                return Metadata.GameType;
            }
            set
            {
                Metadata.SetTag("GameType", Enums.GetGameTypeString(value));
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

        public Board CurrentBoard
        {
            get
            {
                return _currentBoard;
            }
            set
            {
                _currentBoard = value ?? throw new ArgumentNullException(nameof(value));

                if (GameMode == GameMode.Play)
                {
                    GameRecording = new GameRecording(CurrentBoard, GameRecordingSource.Game, GameRecording.Metadata);
                    Metadata.SetTag("Result", CurrentBoard.BoardState.ToString());
                }
            }
        }
        private Board _currentBoard;

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
            GameRecording = new GameRecording(new Board(), GameRecordingSource.Game);
            _currentBoard = GameRecording.Board.Clone();
        }

        public GameSettings(GameRecording gameRecording)
        {
            GameRecording = gameRecording ?? throw new ArgumentNullException(nameof(gameRecording));
            _currentBoard = GameRecording.Board.Clone();
        }

        public GameSettings(Board board, GameMetadata metadata = null)
        {
            if (null == board)
            {
                throw new ArgumentNullException(nameof(board));
            }

            GameRecording = new GameRecording(board, GameRecordingSource.Game, metadata);
            _currentBoard = GameRecording.Board.Clone();
        }

        public GameSettings Clone()
        {
            GameSettings clone = new GameSettings(CurrentBoard, Metadata)
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
            GameSettings clone = new GameSettings(new Board(source.GameType))
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
