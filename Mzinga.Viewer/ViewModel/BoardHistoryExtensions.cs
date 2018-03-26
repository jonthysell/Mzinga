// 
// BoardHistoryExtensions.cs
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
using System.Collections.Generic;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
{
    static class BoardHistoryExtensions
    {
        public static IEnumerable<Tuple<ViewerBoard, BoardHistoryItem>>EnumerateWithBoard(this BoardHistory boardHistory, ViewerBoard currentBoard)
        {
            // Create a copy of the current board
            ViewerBoard board = new ViewerBoard(currentBoard.ToString());

            List<BoardHistoryItem> reversedHistory = new List<BoardHistoryItem>(boardHistory);
            reversedHistory.Reverse();

            // "Undo" moves in the boardHistory
            foreach (BoardHistoryItem item in reversedHistory)
            {
                board.SimulateUndo(item);
            }

            // "Play" forward returning the board state along the way
            foreach (BoardHistoryItem item in boardHistory)
            {
                yield return new Tuple<ViewerBoard, BoardHistoryItem>(board, item);
                board.SimulatePlay(item.Move);
            }
        }
    }
}
