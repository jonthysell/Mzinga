// 
// MainWindow.xaml.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Mzinga.Core;
using Mzinga.Viewer.ViewModel;

namespace Mzinga.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel VM
        {
            get
            {
                return DataContext as MainViewModel;
            }
        }

        public Point CanvasCursorPosition
        {
            get
            {
                Point point = MouseUtils.CorrectGetPosition(BoardCanvas);
                point.X -= CanvasOffsetX;
                point.Y -= CanvasOffsetY;
                return point;
            }
        }

        private double PieceCanvasMargin = 3.0;

        private double CanvasOffsetX = 0.0;
        private double CanvasOffsetY = 0.0;

        private bool RaiseStackedPieces
        {
            get
            {
                return _raiseStackedPieces;
            }
            set
            {
                bool oldValue = _raiseStackedPieces;
                if (oldValue != value)
                {
                    _raiseStackedPieces = value;
                    DrawBoard(LastBoard);
                }
            }
        }
        private bool _raiseStackedPieces;

        private double StackShiftRatio
        {
            get
            {
                return RaiseStackedPieces ? 0.5 : 0.1;
            }
        }

        private Board LastBoard;

        private SolidColorBrush WhiteBrush;
        private SolidColorBrush BlackBrush;

        private SolidColorBrush SelectedMoveEdgeBrush;
        private SolidColorBrush SelectedMoveBodyBrush;

        private SolidColorBrush LastMoveEdgeBrush;

        private SolidColorBrush QueenBeeBrush;
        private SolidColorBrush SpiderBrush;
        private SolidColorBrush BeetleBrush;
        private SolidColorBrush GrasshopperBrush;
        private SolidColorBrush SoldierAntBrush;

        private SolidColorBrush MosquitoBrush;
        private SolidColorBrush LadybugBrush;
        private SolidColorBrush PillbugBrush;

        private SolidColorBrush DisabledPieceBrush;

        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;

            // Init brushes
            WhiteBrush = new SolidColorBrush(Colors.White);
            BlackBrush = new SolidColorBrush(Colors.Black);

            SelectedMoveEdgeBrush = new SolidColorBrush(Colors.Orange);
            SelectedMoveBodyBrush = new SolidColorBrush(Colors.Aqua)
            {
                Opacity = 0.25
            };

            LastMoveEdgeBrush = new SolidColorBrush(Colors.SeaGreen);

            QueenBeeBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 250, G = 167, B = 29, A = 255 });
            SpiderBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 139, G = 63, B = 27, A = 255 });
            BeetleBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 149, G = 101, B = 194, A = 255 });
            GrasshopperBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 65, G = 157, B = 70, A = 255 });
            SoldierAntBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 37, G = 141, B = 193, A = 255 });

            MosquitoBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 111, G = 111, B = 97, A = 255 });
            LadybugBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 211, G = 17, B = 69, A = 255 });
            PillbugBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 30, G = 183, B = 182, A = 255 });

            DisabledPieceBrush = new SolidColorBrush(Colors.LightGray);

            // Bind board updates to VM
            if (null != VM)
            {
                VM.PropertyChanged += VM_PropertyChanged;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            EngineConsoleWindow.Instance.Close();
        }

        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Board":
                case "ValidMoves":
                case "BoardHistory":
                case "TargetMove":
                case "ViewerConfig":
                    AppViewModel.Instance.DoOnUIThread(() =>
                        {
                            DrawBoard(VM.Board);
                        });
                    break;
            }
        }

        private void DrawBoard(Board board)
        {
            BoardCanvas.Children.Clear();
            WhiteHandStackPanel.Children.Clear();
            BlackHandStackPanel.Children.Clear();

            CanvasOffsetX = 0.0;
            CanvasOffsetX = 0.0;

            if (null != board)
            {
                Point minPoint = new Point(double.MaxValue, double.MaxValue);
                Point maxPoint = new Point(double.MinValue, double.MinValue);

                int maxStack;
                int numPieces;
                Dictionary<int, List<Piece>> piecesInPlay = GetPiecesOnBoard(board, out numPieces, out maxStack);

                int whiteHandCount = board.WhiteHand.Count();
                int blackHandCount = board.BlackHand.Count();

                int horizontalPiecesMin = 2 + Math.Max(Math.Max(whiteHandCount, blackHandCount), board.GetWidth());
                int verticalPiecesMin = 1 + Math.Min(whiteHandCount, 1) + Math.Min(blackHandCount, 1) + board.GetHeight();

                double size = 0.5 * Math.Min(BoardCanvas.ActualHeight / verticalPiecesMin, BoardCanvas.ActualWidth / horizontalPiecesMin);

                WhiteHandStackPanel.MinHeight = whiteHandCount > 0 ? (size + PieceCanvasMargin) * 2 : 0;
                BlackHandStackPanel.MinHeight = blackHandCount > 0 ? (size + PieceCanvasMargin) * 2 : 0;

                Position lastMoveStart = VM.AppVM.EngineWrapper.BoardHistory?.LastMove?.OriginalPosition;
                Position lastMoveEnd = VM.AppVM.EngineWrapper.BoardHistory?.LastMove?.Move?.Position;

                PieceName selectedPieceName = VM.AppVM.EngineWrapper.TargetPiece;
                Position targetPosition = VM.AppVM.EngineWrapper.TargetPosition;

                MoveSet validMoves = VM.AppVM.EngineWrapper.ValidMoves;

                HexOrientation hexOrientation = VM.ViewerConfig.HexOrientation;

                // Draw the pieces in play
                for (int stack = 0; stack <= maxStack; stack++)
                {
                    if (piecesInPlay.ContainsKey(stack))
                    {
                        foreach (Piece piece in piecesInPlay[stack])
                        {
                            Position position = piece.Position;

                            if (piece.PieceName == selectedPieceName && null != targetPosition)
                            {
                                position = targetPosition;
                            }

                            Point center = GetPoint(position, size, hexOrientation, true);

                            HexType hexType = (piece.Color == Core.Color.White) ? HexType.WhitePiece : HexType.BlackPiece;

                            Shape hex = GetHex(center, size, hexType, hexOrientation);
                            BoardCanvas.Children.Add(hex);

                            bool disabled = VM.ViewerConfig.DisablePiecesInPlayWithNoMoves && !(null != validMoves && validMoves.Any(m => m.PieceName == piece.PieceName));

                            TextBlock hexText = GetHexText(center, size, piece.PieceName, disabled);
                            BoardCanvas.Children.Add(hexText);

                            minPoint = Min(center, size, minPoint);
                            maxPoint = Max(center, size, maxPoint);
                        }
                    }
                }

                // Draw the pieces in white's hand
                foreach (PieceName pieceName in board.WhiteHand)
                {
                    if (pieceName != selectedPieceName || (pieceName == selectedPieceName && null == targetPosition))
                    {
                        bool disabled = VM.ViewerConfig.DisablePiecesInHandWithNoMoves && !(null != validMoves && validMoves.Any(m => m.PieceName == pieceName));
                        Canvas pieceCanvas = GetPieceInHandCanvas(new Piece(pieceName, board.GetPiecePosition(pieceName)), size, hexOrientation, disabled);
                        WhiteHandStackPanel.Children.Add(pieceCanvas);
                    }
                }

                // Draw the pieces in black's hand
                foreach (PieceName pieceName in board.BlackHand)
                {
                    if (pieceName != selectedPieceName || (pieceName == selectedPieceName && null == targetPosition))
                    {
                        bool disabled = VM.ViewerConfig.DisablePiecesInHandWithNoMoves && !(null != validMoves && validMoves.Any(m => m.PieceName == pieceName));
                        Canvas pieceCanvas = GetPieceInHandCanvas(new Piece(pieceName, board.GetPiecePosition(pieceName)), size, hexOrientation, disabled);
                        BlackHandStackPanel.Children.Add(pieceCanvas);
                    }
                }

                // Highlight last move played
                if (VM.AppVM.ViewerConfig.HighlightLastMovePlayed)
                {
                    // Highlight the lastMove start position
                    if (null != lastMoveStart)
                    {
                        Point center = GetPoint(lastMoveStart, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.LastMove, hexOrientation);
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }

                    // Highlight the lastMove end position
                    if (null != lastMoveEnd)
                    {
                        Point center = GetPoint(lastMoveEnd, size, hexOrientation, true);

                        Shape hex = GetHex(center, size, HexType.LastMove, hexOrientation);
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }
                }

                // Highlight the selected piece
                if (VM.AppVM.ViewerConfig.HighlightTargetMove)
                {
                    if (selectedPieceName != PieceName.INVALID)
                    {
                        Position selectedPiecePosition = board.GetPiecePosition(selectedPieceName);

                        if (null != selectedPiecePosition)
                        {
                            Point center = GetPoint(selectedPiecePosition, size, hexOrientation, true);

                            Shape hex = GetHex(center, size, HexType.SelectedPiece, hexOrientation);
                            BoardCanvas.Children.Add(hex);

                            minPoint = Min(center, size, minPoint);
                            maxPoint = Max(center, size, maxPoint);
                        }
                    }
                }

                // Draw the valid moves for that piece
                if (VM.AppVM.ViewerConfig.HighlightValidMoves)
                {
                    if (selectedPieceName != PieceName.INVALID && null != validMoves)
                    {
                        foreach (Move validMove in validMoves)
                        {
                            if (validMove.PieceName == selectedPieceName)
                            {
                                Point center = GetPoint(validMove.Position, size, hexOrientation);

                                Shape hex = GetHex(center, size, HexType.ValidMove, hexOrientation);
                                BoardCanvas.Children.Add(hex);

                                minPoint = Min(center, size, minPoint);
                                maxPoint = Max(center, size, maxPoint);
                            }
                        }
                    }
                }

                // Highlight the target position
                if (VM.AppVM.ViewerConfig.HighlightTargetMove)
                {
                    if (null != targetPosition)
                    {
                        Point center = GetPoint(targetPosition, size, hexOrientation,  true);

                        Shape hex = GetHex(center, size, HexType.SelectedMove, hexOrientation);
                        BoardCanvas.Children.Add(hex);

                        minPoint = Min(center, size, minPoint);
                        maxPoint = Max(center, size, maxPoint);
                    }
                }

                // Translate everything on the board
                double boardWidth = Math.Abs(maxPoint.X - minPoint.X);
                double boardHeight = Math.Abs(maxPoint.Y - minPoint.Y);

                double boardCenterX = minPoint.X + (boardWidth / 2);
                double boardCenterY = minPoint.Y + (boardHeight / 2);

                double canvasCenterX = BoardCanvas.ActualWidth / 2;
                double canvasCenterY = BoardCanvas.ActualHeight / 2;

                double offsetX = canvasCenterX - boardCenterX;
                double offsetY = canvasCenterY - boardCenterY;

                TranslateTransform translate = new TranslateTransform(offsetX, offsetY);

                foreach (UIElement child in BoardCanvas.Children)
                {
                    if (null != (child as TextBlock)) // Hex labels
                    {
                        Canvas.SetLeft(child, Canvas.GetLeft(child) + offsetX);
                        Canvas.SetTop(child, Canvas.GetTop(child) + offsetY);
                    }
                    else if (null != (child as Shape)) // Hexes
                    {
                        child.RenderTransform = translate;
                    }
                }

                CanvasOffsetX = offsetX;
                CanvasOffsetY = offsetY;

                VM.CanvasHexRadius = size;
            }

            LastBoard = board;
        }

        private Point Min(Point center, double size, Point minPoint)
        {
            double minX = Math.Min(minPoint.X, center.X - size);
            double minY = Math.Min(minPoint.Y, center.Y - size);

            return new Point(minX, minY);
        }

        private Point Max(Point center, double size, Point maxPoint)
        {
            double maxX = Math.Max(maxPoint.X, center.X + size);
            double maxY = Math.Max(maxPoint.Y, center.Y + size);

            return new Point(maxX, maxY);
        }

        private Point GetPoint(Position position, double size, HexOrientation hexOrientation, bool stackShift = false)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            double x = hexOrientation == HexOrientation.FlatTop ? size * 1.5 * position.Q : size * Math.Sqrt(3.0) * (position.Q + (0.5 * position.R));
            double y = hexOrientation == HexOrientation.FlatTop ? size * Math.Sqrt(3.0) * (position.R + (0.5 * position.Q)) : size * 1.5 * position.R;

            if (stackShift && position.Stack > 0)
            {
                x += hexOrientation == HexOrientation.FlatTop ? size * 1.5 * StackShiftRatio * position.Stack : size * Math.Sqrt(3.0) * StackShiftRatio * position.Stack;
                y -= hexOrientation == HexOrientation.FlatTop ? size * Math.Sqrt(3.0) * StackShiftRatio * position.Stack : size * 1.5 * StackShiftRatio * position.Stack;
            }

            return new Point(x, y);
        }

        private Dictionary<int, List<Piece>> GetPiecesOnBoard(Board board, out int numPieces, out int maxStack)
        {
            if (null == board)
            {
                throw new ArgumentNullException("board");
            }

            numPieces = 0;
            maxStack = -1;

            Dictionary<int, List<Piece>> pieces = new Dictionary<int, List<Piece>>();
            pieces[0] = new List<Piece>();

            PieceName targetPieceName = VM.AppVM.EngineWrapper.TargetPiece;
            Position targetPosition = VM.AppVM.EngineWrapper.TargetPosition;

            bool targetPieceInPlay = false;

            // Add pieces already on the board
            foreach (PieceName pieceName in board.PiecesInPlay)
            {
                Position position = board.GetPiecePosition(pieceName);

                if (pieceName == targetPieceName)
                {
                    if (null != targetPosition)
                    {
                        position = targetPosition;
                    }
                    targetPieceInPlay = true;
                }

                int stack = position.Stack;
                maxStack = Math.Max(maxStack, stack);

                if (!pieces.ContainsKey(stack))
                {
                    pieces[stack] = new List<Piece>();
                }

                pieces[stack].Add(new Piece(pieceName, position));
                numPieces++;
            }

            // Add piece being placed on the board
            if (!targetPieceInPlay && null != targetPosition)
            {
                int stack = targetPosition.Stack;
                maxStack = Math.Max(maxStack, stack);

                if (!pieces.ContainsKey(stack))
                {
                    pieces[stack] = new List<Piece>();
                }

                pieces[stack].Add(new Piece(targetPieceName, targetPosition));
                numPieces++;
            }

            return pieces;
        }

        private Shape GetHex(Point center, double size, HexType hexType, HexOrientation hexOrientation)
        {
            if (null == center)
            {
                throw new ArgumentNullException("center");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            double strokeThickness = size / 10;

            Path hex = new Path
            {
                StrokeThickness = strokeThickness
            };

            switch (hexType)
            {
                case HexType.WhitePiece:
                    hex.Fill = WhiteBrush;
                    hex.Stroke = BlackBrush;
                    break;
                case HexType.BlackPiece:
                    hex.Fill = BlackBrush;
                    hex.Stroke = BlackBrush;
                    break;
                case HexType.ValidMove:
                    hex.Fill = SelectedMoveBodyBrush;
                    hex.Stroke = SelectedMoveBodyBrush;
                    break;
                case HexType.SelectedPiece:
                    hex.Stroke = SelectedMoveEdgeBrush;
                    break;
                case HexType.SelectedMove:
                    hex.Fill = SelectedMoveBodyBrush;
                    hex.Stroke = SelectedMoveEdgeBrush;
                    break;
                case HexType.LastMove:
                    hex.Stroke = LastMoveEdgeBrush;
                    break;
            }

            PathGeometry data = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.IsClosed = true;

            double hexRadius = size - 0.75 * strokeThickness;

            for (int i = 0; i <= 6; i++)
            {
                double angle_deg = 60.0 * i + (hexOrientation == HexOrientation.PointyTop ? 30.0 : 0);

                double angle_rad1 = Math.PI / 180 * (angle_deg - 3);
                double angle_rad2 = Math.PI / 180 * (angle_deg + 3);

                Point p1 = new Point(center.X + hexRadius * Math.Cos(angle_rad1), center.Y + hexRadius * Math.Sin(angle_rad1));
                Point p2 = new Point(center.X + hexRadius * Math.Cos(angle_rad2), center.Y + hexRadius * Math.Sin(angle_rad2));

                if (i == 0)
                {
                    figure.StartPoint = p2;
                }
                else
                {
                    figure.Segments.Add(new LineSegment(p1, true));
                    figure.Segments.Add(new ArcSegment() { Point = p2, IsStroked = true, IsSmoothJoin = true, SweepDirection = SweepDirection.Counterclockwise });
                }
            }

            data.Figures.Add(figure);
            hex.Data = data;

            return hex;
        }

        private TextBlock GetHexText(Point center, double size, PieceName pieceName, bool disabled)
        {
            if (null == center)
            {
                throw new ArgumentNullException("center");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            TextBlock hexText = new TextBlock
            {
                Text = EnumUtils.GetShortName(pieceName).Substring(1),
                FontFamily = new FontFamily("Lucida Console")
            };

            switch (EnumUtils.GetBugType(pieceName))
            {
                case BugType.QueenBee:
                    hexText.Foreground = QueenBeeBrush;
                    break;
                case BugType.Spider:
                    hexText.Foreground = SpiderBrush;
                    break;
                case BugType.Beetle:
                    hexText.Foreground = BeetleBrush;
                    break;
                case BugType.Grasshopper:
                    hexText.Foreground = GrasshopperBrush;
                    break;
                case BugType.SoldierAnt:
                    hexText.Foreground = SoldierAntBrush;
                    break;
                case BugType.Mosquito:
                    hexText.Foreground = MosquitoBrush;
                    break;
                case BugType.Ladybug:
                    hexText.Foreground = LadybugBrush;
                    break;
                case BugType.Pillbug:
                    hexText.Foreground = PillbugBrush;
                    break;
            }

            if (disabled)
            {
                hexText.Foreground = MixSolidColorBrushes((SolidColorBrush)hexText.Foreground, DisabledPieceBrush);
            }

            hexText.FontSize = size;

            Canvas.SetLeft(hexText, center.X - (hexText.Text.Length * (hexText.FontSize / 3.5)));
            Canvas.SetTop(hexText, center.Y - (hexText.FontSize / 2.0));

            return hexText;
        }

        private static SolidColorBrush MixSolidColorBrushes(SolidColorBrush b1, SolidColorBrush b2)
        {
            SolidColorBrush result = new SolidColorBrush
            {
                Color = System.Windows.Media.Color.FromScRgb((b1.Color.ScA + b2.Color.ScA) / 2, (b1.Color.ScR + b2.Color.ScR) / 2, (b1.Color.ScG + b2.Color.ScG) / 2, (b1.Color.ScB + b2.Color.ScB) / 2)
            };
            return result;
        }

        private enum HexType
        {
            WhitePiece,
            BlackPiece,
            ValidMove,
            SelectedPiece,
            SelectedMove,
            LastMove,
        }

        private Canvas GetPieceInHandCanvas(Piece piece, double size, HexOrientation hexOrientation, bool disabled)
        {
            Point center = new Point(size, size);

            HexType hexType = (piece.Color == Core.Color.White) ? HexType.WhitePiece : HexType.BlackPiece;

            Shape hex = GetHex(center, size, hexType, hexOrientation);
            TextBlock hexText = GetHexText(center, size, piece.PieceName, disabled);

            Canvas pieceCanvas = new Canvas
            {
                Height = size * 2,
                Width = size * 2,
                Margin = new Thickness(PieceCanvasMargin),
                Background = (piece.Color == Core.Color.White) ? WhiteHandStackPanel.Background : BlackHandStackPanel.Background,

                Name = EnumUtils.GetShortName(piece.PieceName)
            };

            pieceCanvas.Children.Add(hex);
            pieceCanvas.Children.Add(hexText);

            // Add highlight if the piece is selected
            if (VM.AppVM.EngineWrapper.TargetPiece == piece.PieceName)
            {
                Shape highlightHex = GetHex(center, size, HexType.SelectedPiece, hexOrientation);
                pieceCanvas.Children.Add(highlightHex);
            }

            pieceCanvas.MouseLeftButtonUp += PieceCanvas_MouseLeftButtonUp;
            pieceCanvas.MouseRightButtonUp += CancelClick;

            return pieceCanvas;
        }

        private void PieceCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas pieceCanvas = sender as Canvas;

            if (null != pieceCanvas)
            {
                PieceName clickedPiece = EnumUtils.ParseShortName(pieceCanvas.Name);
                VM.PieceClick(clickedPiece);
            }
        }

        private void BoardCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = CanvasCursorPosition;
            VM.CanvasClick(p.X, p.Y);
        }

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            VM.CancelClick();
        }

        private DateTime LastRedrawOnSizeChange = DateTime.Now;

        private void BoardCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DateTime.Now - LastRedrawOnSizeChange > TimeSpan.FromMilliseconds(20))
            {
                DrawBoard(LastBoard);
                LastRedrawOnSizeChange = DateTime.Now;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != VM.AppVM.EngineExceptionOnStart)
            {
                ExceptionUtils.HandleException(new Exception("Unable to start the external engine so used the internal one instead.", VM.AppVM.EngineExceptionOnStart));
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.X)
            {
                RaiseStackedPieces = false;
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.X)
            {
                RaiseStackedPieces = true;
                e.Handled = true;
            }
        }
    }
}
