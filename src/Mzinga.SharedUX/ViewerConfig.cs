// 
// ViewerConfig.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Xml;

using Mzinga.Engine;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.SharedUX
{
    public class ViewerConfig
    {
        public AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public EngineType EngineType
        {
            get
            {
                return _engineType;
            }
            set
            {
                _engineType = value;
                if (_engineType == EngineType.CommandLine && string.IsNullOrWhiteSpace(EngineCommandLine))
                {
                    EngineCommandLine = MzingaEngineCommandLine;
                }
            }
        }
        private EngineType _engineType = EngineType.Internal;

        public string EngineCommandLine
        {
            get
            {
                return _engineCommandLine;
            }
            set
            {
                _engineCommandLine = string.IsNullOrWhiteSpace(value) ? MzingaEngineCommandLine : value.Trim();
            }
        }
        private string _engineCommandLine = MzingaEngineCommandLine;

        public HexOrientation HexOrientation { get; set; } = HexOrientation.PointyTop;

        public NotationType NotationType { get; set; } = NotationType.BoardSpace;

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

        public bool ShowBoardHistory { get; set; } = false;

        public bool ShowMoveCommentary { get; set; } = false;

        public bool FirstRun { get; set; } = true;

        public bool CheckUpdateOnStart { get; set; } = true;

        public GameEngineConfig InternalGameEngineConfig { get; set; } = null;

        public ViewerConfig() { }

        public void LoadConfig(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            using (XmlReader reader = XmlReader.Create(inputStream))
            {
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
                            case "HexOrientation":
                                HexOrientation = ParseEnumValue(reader.ReadElementContentAsString(), HexOrientation);
                                break;
                            case "NotationType":
                                NotationType = ParseEnumValue(reader.ReadElementContentAsString(), NotationType);
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
                                InternalGameEngineConfig?.LoadGameAIConfig(reader.ReadSubtree());
                                break;
                        }
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
            if (null == outputStream)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(outputStream, settings))
            {
                writer.WriteStartElement("MzingaViewer");

                writer.WriteAttributeString("version", AppVM.FullVersion);
                writer.WriteAttributeString("date", DateTime.UtcNow.ToString());

                writer.WriteElementString("EngineType", EngineType.ToString());
                writer.WriteElementString("EngineCommandLine", EngineCommandLine);
                writer.WriteElementString("HexOrientation", HexOrientation.ToString());
                writer.WriteElementString("NotationType", NotationType.ToString());
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
                writer.WriteElementString("ShowBoardHistory", ShowBoardHistory.ToString());
                writer.WriteElementString("ShowMoveCommentary", ShowMoveCommentary.ToString());
                writer.WriteElementString("FirstRun", FirstRun.ToString());
                writer.WriteElementString("CheckUpdateOnStart", CheckUpdateOnStart.ToString());

                InternalGameEngineConfig?.SaveGameAIConfig(writer, "InternalGameAI", ConfigSaveType.BasicOptions);

                writer.WriteEndElement();
            }
        }

        public ViewerConfig Clone()
        {
            ViewerConfig clone = new ViewerConfig
            {
                EngineCommandLine = EngineCommandLine,
                EngineType = EngineType,

                HexOrientation = HexOrientation,
                NotationType = NotationType,

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

                ShowBoardHistory = ShowBoardHistory,
                ShowMoveCommentary = ShowMoveCommentary,

                FirstRun = FirstRun,
                CheckUpdateOnStart = CheckUpdateOnStart,

                InternalGameEngineConfig = InternalGameEngineConfig,
            };

            return clone;
        }

        public void CopyFrom(ViewerConfig config)
        {
            if (null == config)
            {
                throw new ArgumentNullException(nameof(config));
            }

            EngineCommandLine = config.EngineCommandLine;
            EngineType = config.EngineType;

            HexOrientation = config.HexOrientation;
            NotationType = config.NotationType;

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

            ShowBoardHistory = config.ShowBoardHistory;
            ShowMoveCommentary = config.ShowMoveCommentary;

            FirstRun = config.FirstRun;
            CheckUpdateOnStart = config.CheckUpdateOnStart;

            InternalGameEngineConfig = config.InternalGameEngineConfig;
        }

        private const string MzingaEngineCommandLine = "./MzingaEngine";
    }

    public enum HexOrientation
    {
        PointyTop,
        FlatTop,
    }

    public enum NotationType
    {
        BoardSpace,
        Mzinga,
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
