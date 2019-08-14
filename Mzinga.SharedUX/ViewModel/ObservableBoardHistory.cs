// 
// ObservableBoardHistory.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2019 Jon Thysell <http://jonthysell.com>
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
using System.Collections.ObjectModel;
using System.Text;

using GalaSoft.MvvmLight;

using Mzinga.Core;

namespace Mzinga.SharedUX.ViewModel
{
    public class ObservableBoardHistory : ObservableObject
    {
        public AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public ObservableCollection<ObservableBoardHistoryItem> Items { get; private set; } = new ObservableCollection<ObservableBoardHistoryItem>();

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                foreach (ObservableBoardHistoryItem item in Items)
                {
                    sb.AppendLine(item.MoveString);
                }

                return sb.ToString();
            }
        }

        public int CurrentMoveIndex
        {
            get
            {
                return _activeBoardHistory.Count - 1;
            }
            set
            {
                _moveNumberChangedCallback?.Invoke(value + 1);
                RaisePropertyChanged("CurrentMoveIndex");
            }
        }

        internal BoardHistory _boardHistory;
        internal BoardHistory _activeBoardHistory;

        private Action<int> _moveNumberChangedCallback;

        public ObservableBoardHistory(BoardHistory boardHistory, BoardHistory activeBoardHistory = null, Action<int> moveNumberChangedCallback = null)
        {
            _boardHistory = boardHistory ?? throw new ArgumentNullException(nameof(boardHistory));
            _activeBoardHistory = activeBoardHistory ?? boardHistory;
            _moveNumberChangedCallback = moveNumberChangedCallback;

            if (_activeBoardHistory.Count > _boardHistory.Count)
            {
                throw new ArgumentException("Active history has more moves than history.");
            }

            int countWidth = _boardHistory.Count.ToString().Length;

            for (int i = 0; i < _boardHistory.Count; i++)
            {
                BoardHistoryItem item = _boardHistory[i];

                string countString = (i + 1).ToString().PadLeft(countWidth) + ". ";
                string moveString = AppVM.ViewerConfig.NotationType == NotationType.BoardSpace ? NotationUtils.NormalizeBoardSpaceMoveString(item.MoveString) : item.ToString();
                
                bool isActive = i < _activeBoardHistory.Count;
                bool isLastMove = i + 1 == _activeBoardHistory.Count;

                Items.Add(new ObservableBoardHistoryItem(countString + moveString, isActive, isLastMove));
            }
        }
    }

    public class ObservableBoardHistoryItem : ObservableObject
    {
        public string MoveString { get; private set; }

        public bool IsActive { get; private set; }

        public bool IsLastMove { get; private set; }

        public ObservableBoardHistoryItem(string moveString, bool isActive, bool isLastMove)
        {
            MoveString = moveString;
            IsActive = isActive;
            IsLastMove = isLastMove;
        }
    }
}
