// 
// ViewerConfig.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Viewer.ViewModel
{
    public class ViewerConfig
    {
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
        private EngineType _engineType = EngineType.CommandLine;

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

        public HexOrientation HexOrientation
        {
            get
            {
                return _hexOrientation;
            }
            set
            {
                _hexOrientation = value;
                if (_hexOrientation == HexOrientation.FlatTop)
                {
                    NotationType = NotationType.Mzinga;
                }
            }
        }
        private HexOrientation _hexOrientation = HexOrientation.FlatTop;

        public NotationType NotationType
        {
            get
            {
                return _notationType;
            }
            set
            {
                _notationType = value;
                if (_notationType == NotationType.BoardSpace)
                {
                    HexOrientation = HexOrientation.PointyTop;
                }
            }
        }
        private NotationType _notationType = NotationType.Mzinga;

        public bool DisablePiecesInHandWithNoMoves { get; set; } = false;

        public bool DisablePiecesInPlayWithNoMoves { get; set; } = false;

        public bool HighlightTargetMove { get; set; } = true;

        public bool HighlightValidMoves { get; set; } = true;

        public bool HighlightLastMovePlayed { get; set; } = true;

        public bool BlockInvalidMoves { get; set; } = true;

        public bool RequireMoveConfirmation { get; set; } = true;

        public ViewerConfig() { }

        public void LoadConfig(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
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
                                RequireMoveConfirmation = ParseBoolValue(reader.ReadElementContentAsString(), BlockInvalidMoves);
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
            TEnum result;
            return Enum.TryParse(rawValue, out result) ? result : defaultValue;
        }

        private static bool ParseBoolValue(string rawValue, bool defaultValue)
        {
            bool result;
            return bool.TryParse(rawValue, out result) ? result : defaultValue;
        }

        public void SaveConfig(Stream outputStream)
        {
            if (null == outputStream)
            {
                throw new ArgumentNullException("outputStream");
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(outputStream, settings))
            {
                writer.WriteStartElement("Mzinga.Viewer");

                writer.WriteAttributeString("version", AppViewModel.FullVersion);
                writer.WriteAttributeString("date", DateTime.UtcNow.ToString());

                writer.WriteElementString("EngineType", EngineType.ToString());
                writer.WriteElementString("EngineCommandLine", EngineCommandLine);
                writer.WriteElementString("HexOrientation", HexOrientation.ToString());
                writer.WriteElementString("NotationType", NotationType.ToString());
                writer.WriteElementString("DisablePiecesInHandWithNoMoves", DisablePiecesInHandWithNoMoves.ToString());
                writer.WriteElementString("DisablePiecesInPlayWithNoMoves", DisablePiecesInPlayWithNoMoves.ToString());
                writer.WriteElementString("HighlightTargetMove", HighlightTargetMove.ToString());
                writer.WriteElementString("HighlightValidMoves", HighlightValidMoves.ToString());
                writer.WriteElementString("HighlightLastMovePlayed", HighlightLastMovePlayed.ToString());
                writer.WriteElementString("BlockInvalidMoves", BlockInvalidMoves.ToString());
                writer.WriteElementString("RequireMoveConfirmation", RequireMoveConfirmation.ToString());

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

                DisablePiecesInHandWithNoMoves = DisablePiecesInHandWithNoMoves,
                DisablePiecesInPlayWithNoMoves = DisablePiecesInPlayWithNoMoves,

                HighlightTargetMove = HighlightTargetMove,
                HighlightValidMoves = HighlightValidMoves,
                HighlightLastMovePlayed = HighlightLastMovePlayed,

                BlockInvalidMoves = BlockInvalidMoves,
                RequireMoveConfirmation = RequireMoveConfirmation
            };

            return clone;
        }

        public void CopyFrom(ViewerConfig config)
        {
            if (null == config)
            {
                throw new ArgumentNullException("config");
            }

            EngineCommandLine = config.EngineCommandLine;
            EngineType = config.EngineType;

            HexOrientation = config.HexOrientation;
            NotationType = config.NotationType;

            DisablePiecesInHandWithNoMoves = config.DisablePiecesInHandWithNoMoves;
            DisablePiecesInPlayWithNoMoves = config.DisablePiecesInPlayWithNoMoves;

            HighlightTargetMove = config.HighlightTargetMove;
            HighlightValidMoves = config.HighlightValidMoves;
            HighlightLastMovePlayed = config.HighlightLastMovePlayed;

            BlockInvalidMoves = config.BlockInvalidMoves;
            RequireMoveConfirmation = config.RequireMoveConfirmation;
        }

        private const string MzingaEngineCommandLine = "Mzinga.Engine.exe";
    }

    public enum HexOrientation
    {
        FlatTop,
        PointyTop
    }

    public enum NotationType
    {
        Mzinga,
        BoardSpace
    }

    public enum EngineType
    {
        Internal,
        CommandLine,
    }
}
