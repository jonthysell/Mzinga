// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Text;

using GalaSoft.MvvmLight;

using Mzinga.Core;

namespace Mzinga.SharedUX.ViewModel
{
    public class ObservableBoardHistory : ObservableObject
    {
        public static AppViewModel AppVM
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
                if (value >= 0)
                {
                    _moveNumberChangedCallback?.Invoke(value + 1);
                }
                RaisePropertyChanged(nameof(CurrentMoveIndex));
            }
        }

        internal BoardHistory _boardHistory;
        internal BoardHistory _activeBoardHistory;

        private readonly Action<int> _moveNumberChangedCallback;

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
