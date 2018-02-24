// 
// NotationUtils.cs
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

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
{
    public class NotationUtils
    {
        public static string ToBoardSpaceMoveString(ViewerBoard board, Move move)
        {
            if (null == board)
            {
                throw new ArgumentNullException("board");
            }

            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            if (move.IsPass)
            {
                return "pass";
            }


            if (move.Color != board.CurrentTurnColor)
            {
                return null;
            }

            string startPiece = ToBoardSpacePieceName(move.PieceName);

            if (board.CurrentTurn == 0)
            {
                return startPiece;
            }
            else
            {
                string endPiece = "";

                if (move.Position.Stack > 0)
                {
                    // On top of board
                    PieceName pieceBelow = board.GetPiece(move.Position.GetBelow());
                    endPiece = ToBoardSpacePieceName(pieceBelow);
                }
                else
                {
                    // Find neighbor to move.Position
                    foreach (Direction dir in EnumUtils.Directions)
                    {
                        // Found a neighbor!

                        Position pos = move.Position.NeighborAt(dir);
                        PieceName neighbor = board.GetPieceOnTop(pos);

                        if (neighbor == move.PieceName)
                        {
                            Position posBelow = board.GetPiecePosition(neighbor).GetBelow();
                            neighbor = null != posBelow ? board.GetPiece(posBelow) : PieceName.INVALID;
                        }

                        if (neighbor != PieceName.INVALID)
                        {
                            endPiece = ToBoardSpacePieceName(neighbor);

                            switch (dir)
                            {
                                case Direction.Up:
                                    endPiece = endPiece + @"\";
                                    break;
                                case Direction.UpRight:
                                    endPiece = @"/" + endPiece;
                                    break;
                                case Direction.DownRight:
                                    endPiece = @"-" + endPiece;
                                    break;
                                case Direction.Down:
                                    endPiece = @"\" + endPiece;
                                    break;
                                case Direction.DownLeft:
                                    endPiece = endPiece + @"/";
                                    break;
                                case Direction.UpLeft:
                                    endPiece = endPiece + @"-";
                                    break;
                            }

                            break;
                        }
                    }
                }

                return string.IsNullOrWhiteSpace(endPiece) ? startPiece : string.Format("{0} {1}", startPiece, endPiece);
            }
        }
        
        public static string ToBoardSpacePieceName(PieceName pieceName)
        {
            string name = EnumUtils.GetShortName(pieceName);

            if (null != name && name.Length > 0)
            {
                name = name[0].ToString().ToLower() + name.Substring(1);
            }

            return name;
        }
    }
}
