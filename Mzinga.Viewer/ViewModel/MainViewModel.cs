// 
// MainViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016 Jon Thysell <http://jonthysell.com>
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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public string Title
        {
            get
            {
                return AppVM.ProgramTitle;
            }
        }

        public Board Board
        {
            get
            {
                return AppVM.EngineWrapper.Board;
            }
        }

        public string EngineOutputText
        {
            get
            {
                return AppVM.EngineWrapper.EngineText;
            }
        }

        public string EngineInputText
        {
            get
            {
                return _engineInputText;
            }
            set
            {
                _engineInputText = value;
                RaisePropertyChanged("EngineInputText");
                RaisePropertyChanged("SendEngineCommand");
            }
        }
        private string _engineInputText = "";

        public string TargetPiece
        {
            get
            {
                return EnumUtils.GetShortName(AppVM.EngineWrapper.TargetPiece);
            }
        }

        public string TargetPosition
        {
            get
            {
                Position pos = AppVM.EngineWrapper.TargetPosition;
                return (null != pos) ? pos.ToString() : "";
            }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    value = " ";
                }
                _statusText = value;
                RaisePropertyChanged("StatusText");
            }
        }
        private string _statusText = " ";

        #region Canvas properties

        public double CanvasHexRadius
        {
            get
            {
                return _canvasHexRadius;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _canvasHexRadius = value;
                RaisePropertyChanged("CanvasHexRadius");
            }
        }
        private double _canvasHexRadius = 20;

        public double CanvasCursorX
        {
            get
            {
                return _canvasCursorX;
            }
            private set
            {
                _canvasCursorX = value;
                RaisePropertyChanged("CanvasCursorX");
            }
        }
        private double _canvasCursorX;

        public double CanvasCursorY
        {
            get
            {
                return _canvasCursorY;
            }
            private set
            {
                _canvasCursorY = value;
                RaisePropertyChanged("CanvasCursorY");
            }
        }
        private double _canvasCursorY;

        #endregion

        public RelayCommand NewGame
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.NewGame();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }

        public RelayCommand PlayTarget
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.PlayTargetMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return AppVM.EngineWrapper.CanPlayTargetMove;
                });
            }
        }

        public RelayCommand Pass
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.Pass();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return AppVM.EngineWrapper.CanPass;
                });
            }
        }

        public RelayCommand UndoLastMove
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.UndoLastMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return AppVM.EngineWrapper.CanUndoLastMove;
                });
            }
        }

        public RelayCommand FindBestMove
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.FindBestMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return AppVM.EngineWrapper.GameInProgress;
                });
            }
        }

        public RelayCommand PlayBestMove
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.PlayBestMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return AppVM.EngineWrapper.GameInProgress;
                });
            }
        }

        public RelayCommand SendEngineCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.SendCommand(EngineInputText);                      
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                    finally
                    {
                        EngineInputText = "";
                    }
                }, () =>
                {
                    return !String.IsNullOrWhiteSpace(EngineInputText);
                });
            }
        }

        public MainViewModel()
        {
            AppVM.EngineWrapper.BoardUpdated += (board) =>
            {
                RaisePropertyChanged("Board");
                RaisePropertyChanged("Pass");
                RaisePropertyChanged("PlayBestMove");
                RaisePropertyChanged("FindBestMove");
                RaisePropertyChanged("UndoLastMove");
                UpdateStatusText();
            };

            AppVM.EngineWrapper.EngineTextUpdated += (engineText) =>
            {
                RaisePropertyChanged("EngineOutputText");
            };

            AppVM.EngineWrapper.TargetPieceUpdated += (pieceName) =>
            {
                RaisePropertyChanged("TargetPiece");
                RaisePropertyChanged("PlayTarget");
                UpdateStatusText();
            };

            AppVM.EngineWrapper.TargetPositionUpdated += (position) =>
            {
                RaisePropertyChanged("TargetPosition");
                RaisePropertyChanged("PlayTarget");
                UpdateStatusText();
            };
        }

        internal void CanvasClick(double cursorX, double cursorY)
        {
            CanvasCursorX = cursorX;
            CanvasCursorY = cursorY;

            PieceName clickedPiece = AppVM.EngineWrapper.GetPieceAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius);
            Position clickedPosition = AppVM.EngineWrapper.GetTargetPositionAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius);

            if (AppVM.EngineWrapper.TargetPiece == PieceName.INVALID && clickedPiece != PieceName.INVALID)
            {
                // No piece seleected, select it
                AppVM.EngineWrapper.TargetPiece = clickedPiece;
            }
            else if (AppVM.EngineWrapper.TargetPiece != PieceName.INVALID)
            {
                // Piece is selected
                if (clickedPiece == AppVM.EngineWrapper.TargetPiece)
                {
                    // Unselect piece
                    AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
                }
                else
                {
                    // Get the move with the clicked position
                    Move targetMove = new Move(AppVM.EngineWrapper.TargetPiece, clickedPosition);
                    if (AppVM.EngineWrapper.CanPlayMove(targetMove))
                    {
                        // Move is valid, select position
                        AppVM.EngineWrapper.TargetPosition = clickedPosition;
                    }
                    else
                    {
                        // Move is invalid, (un)select clicked piece
                        AppVM.EngineWrapper.TargetPiece = clickedPiece;
                    }
                }
            }
        }

        internal void PieceClick(PieceName clickedPiece)
        {
            if (AppVM.EngineWrapper.TargetPiece == clickedPiece)
            {
                clickedPiece = PieceName.INVALID;
            }

            AppVM.EngineWrapper.TargetPiece = clickedPiece;
        }

        private void UpdateStatusText()
        {
            if (null != Board)
            {
                StatusText = String.Format("BoardState: {0} CurrentTurnColor: {1} CurrentTurn: {2} TargetPiece: {3} TargetPosition: {4}",
                        Board.BoardState.ToString(),
                        Board.CurrentTurnColor.ToString(),
                        Board.CurrentTurn,
                        TargetPiece,
                        TargetPosition);
            }
        }
    }
}
