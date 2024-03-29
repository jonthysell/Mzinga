﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;
using System.Xml;

using Mzinga.Engine;
using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer
{
    public class ViewerConfig
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public EngineType EngineType { get; set; } = EngineType.Internal;

        public string EngineCommandLine
        {
            get
            {
                return _engineCommandLine;
            }
            set
            {
                _engineCommandLine = value?.Trim();
            }
        }
        private string _engineCommandLine = "";

        public VisualTheme VisualTheme { get; set; } = VisualTheme.Light;

        public HexOrientation HexOrientation { get; set; } = HexOrientation.PointyTop;

        public PieceStyle PieceStyle { get; set; } = PieceStyle.Graphical;

        public bool PieceColors { get; set; } = true;

        public bool DisablePiecesInHandWithNoMoves { get; set; } = false;

        public bool DisablePiecesInPlayWithNoMoves { get; set; } = false;

        public bool HighlightTargetMove { get; set; } = true;

        public bool HighlightValidMoves { get; set; } = true;

        public bool HighlightLastMovePlayed { get; set; } = true;

        public bool BlockInvalidMoves { get; set; } = true;

        public bool RequireMoveConfirmation { get; set; } = false;

        public bool AddPieceNumbers { get; set; } = false;

        public bool StackPiecesInHand { get; set; } = true;

        public bool PlaySoundEffects { get; set; } = true;

        public bool AutoCenterBoard { get; set; } = true;

        public bool AutoZoomBoard { get; set; } = true;

        public bool ShowBoardHistory { get; set; } = false;

        public bool ShowMoveCommentary { get; set; } = false;

        public bool FirstRun { get; set; } = true;

        public bool CheckUpdateOnStart { get; set; } = true;

        public EngineConfig InternalEngineConfig { get; set; } = null;

        public ViewerConfig() { }

        public void LoadConfig(Stream inputStream)
        {
            if (inputStream is null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            using XmlReader reader = XmlReader.Create(inputStream);
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "EngineType":
                            EngineType = ParseEnumValue(reader.ReadElementContentAsString(), EngineType);
                            break;
                        case "EngineCommandLine":
                            EngineCommandLine = ParseStringValue(reader.ReadElementContentAsString(), EngineCommandLine);
                            break;
                        case "VisualTheme":
                            VisualTheme = ParseEnumValue(reader.ReadElementContentAsString(), VisualTheme);
                            break;
                        case "HexOrientation":
                            HexOrientation = ParseEnumValue(reader.ReadElementContentAsString(), HexOrientation);
                            break;
                        case "PieceStyle":
                            PieceStyle = ParseEnumValue(reader.ReadElementContentAsString(), PieceStyle);
                            break;
                        case "PieceColors":
                            PieceColors = ParseBoolValue(reader.ReadElementContentAsString(), PieceColors);
                            break;
                        case "DisablePiecesInHandWithNoMoves":
                            DisablePiecesInHandWithNoMoves = ParseBoolValue(reader.ReadElementContentAsString(), DisablePiecesInHandWithNoMoves);
                            break;
                        case "DisablePiecesInPlayWithNoMoves":
                            DisablePiecesInPlayWithNoMoves = ParseBoolValue(reader.ReadElementContentAsString(), DisablePiecesInPlayWithNoMoves);
                            break;
                        case "HighlightTargetMove":
                            HighlightTargetMove = ParseBoolValue(reader.ReadElementContentAsString(), HighlightTargetMove);
                            break;
                        case "HighlightValidMoves":
                            HighlightValidMoves = ParseBoolValue(reader.ReadElementContentAsString(), HighlightValidMoves);
                            break;
                        case "HighlightLastMovePlayed":
                            HighlightLastMovePlayed = ParseBoolValue(reader.ReadElementContentAsString(), HighlightLastMovePlayed);
                            break;
                        case "BlockInvalidMoves":
                            BlockInvalidMoves = ParseBoolValue(reader.ReadElementContentAsString(), BlockInvalidMoves);
                            break;
                        case "RequireMoveConfirmation":
                            RequireMoveConfirmation = ParseBoolValue(reader.ReadElementContentAsString(), RequireMoveConfirmation);
                            break;
                        case "StackPiecesInHand":
                            StackPiecesInHand = ParseBoolValue(reader.ReadElementContentAsString(), StackPiecesInHand);
                            break;
                        case "AddPieceNumbers":
                            AddPieceNumbers = ParseBoolValue(reader.ReadElementContentAsString(), AddPieceNumbers);
                            break;
                        case "PlaySoundEffects":
                            PlaySoundEffects = ParseBoolValue(reader.ReadElementContentAsString(), PlaySoundEffects);
                            break;
                        case "AutoCenterBoard":
                            AutoCenterBoard = ParseBoolValue(reader.ReadElementContentAsString(), AutoCenterBoard);
                            break;
                        case "AutoZoomBoard":
                            AutoZoomBoard = ParseBoolValue(reader.ReadElementContentAsString(), AutoZoomBoard);
                            break;
                        case "ShowBoardHistory":
                            ShowBoardHistory = ParseBoolValue(reader.ReadElementContentAsString(), ShowBoardHistory);
                            break;
                        case "ShowMoveCommentary":
                            ShowMoveCommentary = ParseBoolValue(reader.ReadElementContentAsString(), ShowMoveCommentary);
                            break;
                        case "FirstRun":
                            FirstRun = ParseBoolValue(reader.ReadElementContentAsString(), FirstRun);
                            break;
                        case "CheckUpdateOnStart":
                            CheckUpdateOnStart = ParseBoolValue(reader.ReadElementContentAsString(), CheckUpdateOnStart);
                            break;
                        case "InternalGameAI":
                            InternalEngineConfig?.LoadGameAIConfig(reader.ReadSubtree());
                            break;
                    }
                }
            }
        }

        private static string ParseStringValue(string rawValue, string defaultValue)
        {
            return !string.IsNullOrWhiteSpace(rawValue) ? rawValue.Trim() : defaultValue;
        }

        private static TEnum ParseEnumValue<TEnum>(string rawValue, TEnum defaultValue) where TEnum : struct, IConvertible
        {
            return Enum.TryParse(rawValue, out TEnum result) ? result : defaultValue;
        }

        private static bool ParseBoolValue(string rawValue, bool defaultValue)
        {
            return bool.TryParse(rawValue, out bool result) ? result : defaultValue;
        }

        public void SaveConfig(Stream outputStream)
        {
            if (outputStream is null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using XmlWriter writer = XmlWriter.Create(outputStream, settings);
            writer.WriteStartElement("MzingaViewer");

            writer.WriteAttributeString("version", AppVM.FullVersion);
            writer.WriteAttributeString("date", DateTime.UtcNow.ToString());

            writer.WriteElementString("EngineType", EngineType.ToString());
            writer.WriteElementString("EngineCommandLine", EngineCommandLine);
            writer.WriteElementString("VisualTheme", VisualTheme.ToString());
            writer.WriteElementString("HexOrientation", HexOrientation.ToString());
            writer.WriteElementString("PieceStyle", PieceStyle.ToString());
            writer.WriteElementString("PieceColors", PieceColors.ToString());
            writer.WriteElementString("DisablePiecesInHandWithNoMoves", DisablePiecesInHandWithNoMoves.ToString());
            writer.WriteElementString("DisablePiecesInPlayWithNoMoves", DisablePiecesInPlayWithNoMoves.ToString());
            writer.WriteElementString("HighlightTargetMove", HighlightTargetMove.ToString());
            writer.WriteElementString("HighlightValidMoves", HighlightValidMoves.ToString());
            writer.WriteElementString("HighlightLastMovePlayed", HighlightLastMovePlayed.ToString());
            writer.WriteElementString("BlockInvalidMoves", BlockInvalidMoves.ToString());
            writer.WriteElementString("RequireMoveConfirmation", RequireMoveConfirmation.ToString());
            writer.WriteElementString("AddPieceNumbers", AddPieceNumbers.ToString());
            writer.WriteElementString("StackPiecesInHand", StackPiecesInHand.ToString());
            writer.WriteElementString("PlaySoundEffects", PlaySoundEffects.ToString());
            writer.WriteElementString("AutoCenterBoard", AutoCenterBoard.ToString());
            writer.WriteElementString("AutoZoomBoard", AutoZoomBoard.ToString());
            writer.WriteElementString("ShowBoardHistory", ShowBoardHistory.ToString());
            writer.WriteElementString("ShowMoveCommentary", ShowMoveCommentary.ToString());
            writer.WriteElementString("FirstRun", FirstRun.ToString());
            writer.WriteElementString("CheckUpdateOnStart", CheckUpdateOnStart.ToString());

            InternalEngineConfig?.SaveGameAIConfig(writer, "InternalGameAI", ConfigSaveType.BasicOptions);

            writer.WriteEndElement();
        }

        public ViewerConfig Clone()
        {
            ViewerConfig clone = new ViewerConfig
            {
                EngineCommandLine = EngineCommandLine,
                EngineType = EngineType,

                VisualTheme = VisualTheme,

                HexOrientation = HexOrientation,

                PieceStyle = PieceStyle,
                PieceColors = PieceColors,

                DisablePiecesInHandWithNoMoves = DisablePiecesInHandWithNoMoves,
                DisablePiecesInPlayWithNoMoves = DisablePiecesInPlayWithNoMoves,

                HighlightTargetMove = HighlightTargetMove,
                HighlightValidMoves = HighlightValidMoves,
                HighlightLastMovePlayed = HighlightLastMovePlayed,

                BlockInvalidMoves = BlockInvalidMoves,
                RequireMoveConfirmation = RequireMoveConfirmation,

                AddPieceNumbers = AddPieceNumbers,
                StackPiecesInHand = StackPiecesInHand,

                PlaySoundEffects = PlaySoundEffects,

                AutoCenterBoard = AutoCenterBoard,
                AutoZoomBoard = AutoZoomBoard,

                ShowBoardHistory = ShowBoardHistory,
                ShowMoveCommentary = ShowMoveCommentary,

                FirstRun = FirstRun,
                CheckUpdateOnStart = CheckUpdateOnStart,

                InternalEngineConfig = InternalEngineConfig,
            };

            return clone;
        }

        public void CopyFrom(ViewerConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            EngineCommandLine = config.EngineCommandLine;
            EngineType = config.EngineType;

            VisualTheme = config.VisualTheme;

            HexOrientation = config.HexOrientation;

            PieceStyle = config.PieceStyle;
            PieceColors = config.PieceColors;

            DisablePiecesInHandWithNoMoves = config.DisablePiecesInHandWithNoMoves;
            DisablePiecesInPlayWithNoMoves = config.DisablePiecesInPlayWithNoMoves;

            HighlightTargetMove = config.HighlightTargetMove;
            HighlightValidMoves = config.HighlightValidMoves;
            HighlightLastMovePlayed = config.HighlightLastMovePlayed;

            BlockInvalidMoves = config.BlockInvalidMoves;
            RequireMoveConfirmation = config.RequireMoveConfirmation;

            AddPieceNumbers = config.AddPieceNumbers;
            StackPiecesInHand = config.StackPiecesInHand;

            PlaySoundEffects = config.PlaySoundEffects;

            AutoCenterBoard = config.AutoCenterBoard;
            AutoZoomBoard = config.AutoZoomBoard;

            ShowBoardHistory = config.ShowBoardHistory;
            ShowMoveCommentary = config.ShowMoveCommentary;

            FirstRun = config.FirstRun;
            CheckUpdateOnStart = config.CheckUpdateOnStart;

            InternalEngineConfig = config.InternalEngineConfig;
        }
    }

    public enum VisualTheme
    {
        Light,
        Dark,
    }

    public enum HexOrientation
    {
        PointyTop,
        FlatTop,
    }

    public enum EngineType
    {
        Internal,
        CommandLine,
    }

    public enum PieceStyle
    {
        Graphical,
        Text,
    }
}
