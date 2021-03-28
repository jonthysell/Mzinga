// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Text;

namespace Mzinga.Core
{
    public readonly struct Move
    {
        public readonly PieceName PieceName;
        public readonly Position Source;
        public readonly Position Destination;

        public Move(PieceName pieceName, Position source, Position desintation)
        {
            PieceName = pieceName;
            Source = source;
            Destination = desintation;
        }

        public static readonly Move PassMove = new Move(PieceName.INVALID, Position.NullPosition, Position.NullPosition);

        public override bool Equals(object? obj)
        {
            return obj is Move move && this == move;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PieceName, Destination);
        }

        public static bool operator ==(Move lhs, Move rhs)
        {
            return lhs.PieceName == rhs.PieceName && lhs.Source == rhs.Source && lhs.Destination == rhs.Destination;
        }

        public static bool operator !=(Move lhs, Move rhs) => !(lhs == rhs);

        public static string BuildMoveString(bool isPass, PieceName startPiece, char beforeSeperator, PieceName endPiece, char afterSeperator)
        {
            if (isPass)
            {
                return PassString;
            }

            var sb = new StringBuilder();

            sb.Append(startPiece.ToString());

            if (endPiece != PieceName.INVALID)
            {
                sb.Append(' ');
                if (beforeSeperator != '\0')
                {
                    sb.Append($"{beforeSeperator}{endPiece}");
                }
                else if (afterSeperator != '\0')
                {
                    sb.Append($"{endPiece}{afterSeperator}");
                }
                else
                {
                    sb.Append(endPiece.ToString());
                }
            }

            return sb.ToString();
        }

        public static bool TryNormalizeMoveString(string moveString, out string? result)
        {
            if (TryNormalizeMoveString(moveString, out bool isPass, out PieceName startPiece, out char beforeSeperator, out PieceName endPiece, out char afterSeperator))
            {
                result = BuildMoveString(isPass, startPiece, beforeSeperator, endPiece, afterSeperator);
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryNormalizeMoveString(string moveString, out bool isPass, out PieceName startPiece, out char beforeSeperator, out PieceName endPiece, out char afterSeperator)
        {
            isPass = false;
            startPiece = PieceName.INVALID;
            beforeSeperator = '\0';
            endPiece = PieceName.INVALID;
            afterSeperator = '\0';

            var piece1 = new StringBuilder();
            var piece2 = new StringBuilder();

            int itemsFound = 0;
            for (int i = 0; i < moveString.Length; i++)
            {
                if (itemsFound == 0 && moveString[i] != ' ')
                {
                    // Start of piece1, save and bump
                    piece1.Append(moveString[i]);
                    itemsFound++;
                }
                else if (itemsFound == 1)
                {
                    if (moveString[i] != ' ')
                    {
                        // Still part of piece1
                        piece1.Append(moveString[i]);
                    }
                    else
                    {
                        // Skip, looking for beforeSeperator now
                        itemsFound++;
                    }
                }
                else if (itemsFound == 2)
                {
                    if (moveString[i] != ' ')
                    {
                        // First non-space
                        if (moveString[i] == '-' || moveString[i] == '/' || moveString[i] == '\\')
                        {
                            // beforeSeparator found
                            beforeSeperator = moveString[i];
                        }
                        else
                        {
                            // Start of piece2
                            piece2.Append(moveString[i]);
                        }
                        itemsFound++;
                    }
                }
                else if (itemsFound == 3)
                {
                    if (moveString[i] != ' ')
                    {
                        if (moveString[i] == '-' || moveString[i] == '/' || moveString[i] == '\\')
                        {
                            // afterSeperator found
                            afterSeperator = moveString[i];
                            break;
                        }
                        else
                        {
                            // Still of piece2
                            piece2.Append(moveString[i]);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var piece1Str = piece1.ToString();

            if (string.Equals(piece1Str, PassString, StringComparison.InvariantCultureIgnoreCase))
            {
                isPass = true;
                return true;
            }

            if (Enum.TryParse(piece1Str, true, out startPiece))
            {
                var piece2Str = piece2.ToString();

                if (!Enum.TryParse(piece2Str, true, out endPiece) || endPiece == PieceName.INVALID)
                {
                    endPiece = PieceName.INVALID;
                    beforeSeperator = '\0';
                    afterSeperator = '\0';
                }
                else if (beforeSeperator != '\0')
                {
                    afterSeperator = '\0';
                }

                return true;
            }

            isPass = false;
            startPiece = PieceName.INVALID;
            beforeSeperator = '\0';
            endPiece = PieceName.INVALID;
            afterSeperator = '\0';
            return false;
        }

        public const string PassString = "pass";
    }
}