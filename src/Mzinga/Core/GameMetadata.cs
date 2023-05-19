// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Mzinga.Core
{
    public class GameMetadata
    {
        #region Mandatory Tags

        public GameType GameType { get; private set; } = GameType.Base;

        public string Event { get; private set; } = ""; // Event name
        public string Site { get; private set; } = ""; // City, Region COUNTRY
        public string Date { get; private set; } = ""; // Date played in yyyy.MM.dd
        public string Round { get; private set; } = ""; // Round number
        public string White { get; private set; } = ""; // White player Lastname, Firstname
        public string Black { get; private set; } = ""; // Black player Lastname, Firstname

        public BoardState Result { get; private set; } = BoardState.NotStarted;

        #endregion

        #region Optional Tags

        public IReadOnlyDictionary<string, string> OptionalTags
        {
            get
            {
                return _optionalTags;
            }
        }

        private readonly Dictionary<string, string> _optionalTags = new Dictionary<string, string>();

        #endregion

        #region Move Commentary

        public IReadOnlyDictionary<int, string?> MoveCommentary
        {
            get
            {
                return _moveCommentary;
            }
        }
        private readonly Dictionary<int, string?> _moveCommentary = new Dictionary<int, string?>();

        #endregion

        public void Clear()
        {
            Event = "";
            Site = "";
            Date = "";
            Round = "";
            White = "";
            Black = "";

            GameType = GameType.Base;
            Result = BoardState.NotStarted;

            _optionalTags.Clear();
        }

        public string? GetTag(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            switch (key)
            {
                case "Event":
                    return Event;
                case "Site":
                    return Site;
                case "Date":
                    return Date;
                case "Round":
                    return Round;
                case "White":
                    return White;
                case "Black":
                    return Black;
                case "GameType":
                    return Enums.GetGameTypeString(GameType);
                case "Result":
                    return Result.ToString();
            }

            if (_optionalTags.TryGetValue(key, out string? value))
            {
                return value;
            }

            return null;
        }

        public void SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            value = value is not null ? value.Replace("\"", "").Trim() : "";

            switch (key)
            {
                case "Event":
                    Event = value;
                    break;
                case "Site":
                    Site = value;
                    break;
                case "Date":
                    Date = value;
                    break;
                case "Round":
                    Round = value;
                    break;
                case "White":
                    White = value;
                    break;
                case "Black":
                    Black = value;
                    break;
                case "GameType":
                    _ = Enums.TryParse(value, out GameType result);
                    GameType = result;
                    break;
                case "Result":
                    Result = (BoardState)Enum.Parse(typeof(BoardState), value);
                    break;
                default:
                    _optionalTags[key] = value;
                    break;
            }
        }

        public void SetMoveCommentary(int moveNum, string? commentary)
        {
            if (moveNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(moveNum));
            }

            _moveCommentary[moveNum] = commentary?.Replace("{", "").Replace("}", "");
        }

        public string? GetMoveCommentary(int moveNum)
        {
            if (moveNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(moveNum));
            }

            if (_moveCommentary.TryGetValue(moveNum, out string? commentary))
            {
                return commentary;
            }

            return null;
        }

        public GameMetadata Clone()
        {
            GameMetadata clone = new GameMetadata()
            {
                Event = Event,
                Site = Site,
                Date = Date,
                Round = Round,
                White = White,
                Black = Black,

                GameType = GameType,
                Result = Result,
            };

            foreach (KeyValuePair<string, string> kvp in OptionalTags)
            {
                clone._optionalTags.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<int, string?> kvp in MoveCommentary)
            {
                clone._moveCommentary.Add(kvp.Key, kvp.Value);
            }

            return clone;
        }

        public void CopyFrom(GameMetadata metadata)
        {
            if (metadata is null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Event = metadata.Event;
            Site = metadata.Site;
            Date = metadata.Date;
            Round = metadata.Round;
            White = metadata.White;
            Black = metadata.Black;

            GameType = metadata.GameType;
            Result = metadata.Result;

            foreach (KeyValuePair<string, string> kvp in metadata.OptionalTags)
            {
                _optionalTags[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<int, string?> kvp in metadata.MoveCommentary)
            {
                _moveCommentary[kvp.Key] = kvp.Value;
            }
        }
    }
}
