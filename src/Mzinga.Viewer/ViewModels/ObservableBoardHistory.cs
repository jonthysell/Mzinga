// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModels
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
                return _currentMoveIndex;
            }
            set
            {
                if (value != _currentMoveIndex)
                {
                    _currentMoveIndex = value;
                    UpdateItems();
                    OnPropertyChanged(nameof(CurrentMoveIndex));
                }
            }
        }
        private int _currentMoveIndex;

        internal readonly BoardHistory BoardHistory;

        public ObservableBoardHistory(BoardHistory boardHistory, int currentMoveIndex)
        {
            BoardHistory = boardHistory ?? throw new ArgumentNullException(nameof(boardHistory));
            _currentMoveIndex = currentMoveIndex < boardHistory.Count ? currentMoveIndex : throw new ArgumentOutOfRangeException(nameof(currentMoveIndex));

            int countWidth = BoardHistory.Count.ToString().Length;

            for (int i = 0; i < BoardHistory.Count; i++)
            {
                BoardHistoryItem item = BoardHistory[i];

                string countString = (i + 1).ToString().PadLeft(countWidth) + ". ";
                string moveString = item.MoveString;

                bool isActive = i <= _currentMoveIndex;
                bool isLastMove = i == _currentMoveIndex;

                Items.Add(new ObservableBoardHistoryItem(countString + moveString, isActive, isLastMove));
            }
        }

        public ObservableBoardHistory(BoardHistory boardHistory) : this(boardHistory, (boardHistory?.Count ?? 0) - 1) { }

        private void UpdateItems()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                UpdateItem(i);
            }
        }

        private void UpdateItem(int moveIndex)
        {
            Items[moveIndex].IsActive = moveIndex <= _currentMoveIndex;
            Items[moveIndex].IsLastMove = moveIndex == _currentMoveIndex;
        }
    }

    public class ObservableBoardHistoryItem : ObservableObject
    {
        public string MoveString
        {
            get
            {
                return _moveString;
            }
            set
            {
                _moveString = value;
                OnPropertyChanged(nameof(MoveString));
            }
        }
        private string _moveString;

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        private bool _isActive;

        public bool IsLastMove
        {
            get
            {
                return _isLastMove;
            }
            set
            {
                _isLastMove = value;
                OnPropertyChanged(nameof(IsLastMove));
            }
        }
        private bool _isLastMove;

        public ObservableBoardHistoryItem(string moveString, bool isActive, bool isLastMove)
        {
            _moveString = moveString;
            _isActive = isActive;
            _isLastMove = isLastMove;
        }
    }
}
